using AutoMapper;
using FluentValidation;
using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Application.Exceptions;
using Gbm.Challenge.Domain.Common;
using Gbm.Challenge.Domain.Common.Settings;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Models.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gbm.Challenge.Application.Features.Accounts.Commands;

public class CreateOrderCommand : IRequest<CurrentBalanceDTO>
{
    public int AccountId { get; set; }
    public OrderDTO Order { get; set; }

    public CreateOrderCommand(int accountId, OrderDTO order)
    {
        AccountId = accountId;
        Order = order;
    }

    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(cmd => cmd.AccountId).NotEmpty().WithMessage("The account ID is required.");
            RuleFor(cmd => cmd.Order).NotNull().WithMessage("The order to create is required.");
            RuleFor(cmd => cmd.Order.Timestamp).NotEmpty().WithMessage("The timestamp is required.");
            RuleFor(cmd => cmd.Order.IssuerName).NotEmpty().WithMessage("The issuer is required.");
            RuleFor(cmd => cmd.Order.TotalShares).NotEmpty().WithMessage("The number of shares is required.");
            RuleFor(cmd => cmd.Order.SharePrice).NotEmpty().WithMessage("The share price is required.");
        }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CurrentBalanceDTO>
    {
        private readonly IAccountRepository _repository;
        private readonly IMapper _mapper;
        private readonly IOptions<GbmChallengeSettings> _settings;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IAccountRepository repository,
            IMapper mapper,
            IOptions<GbmChallengeSettings> settings,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CurrentBalanceDTO> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = _mapper.Map<Order>(request.Order);
            var account = await _repository.GetByIdAsync(request.AccountId, true);
            if (account is null)
            {
                _logger.LogDebug("Account {id} was not found on DB.", request.AccountId);
                throw new NotFoundException(nameof(InvestmentAccount), request.AccountId);
            }

            // Business rules validations
            List<string> businessErrors = new();

            // Insufficient balance
            var totalOrderAmount = order.TotalOrderAmount;
            if (order.Operation == OperationType.Buy && totalOrderAmount > account.Cash)
            {
                _logger.LogDebug("{msg} Account:{account}, Order:{order}", DomainConstants.InsufficientBalance, account.Cash, totalOrderAmount);
                businessErrors.Add(DomainConstants.InsufficientBalance);
            }

            // Insufficient stocks
            var accountStocks = account.GetStocksCount(order.IssuerName);
            if (order.Operation == OperationType.Sell && accountStocks < order.TotalShares)
            {
                _logger.LogDebug("{msg} Account:{account}, Order:{order}", DomainConstants.InsufficientStocks, accountStocks, order.TotalShares);
                businessErrors.Add(DomainConstants.InsufficientStocks);
            }

            // Duplicated operation
            var duplicate = account.Orders.FirstOrDefault(
                            o => o.Operation == order.Operation
                              && o.IssuerName == order.IssuerName
                              && o.SharePrice == order.SharePrice
                              && Math.Abs((o.Timestamp - order.Timestamp).TotalMinutes) < _settings.Value.DuplicatedOrderThreshold);
            if (duplicate != null)
            {
                _logger.LogDebug("{msg} Account:{account}, Order:{order}", DomainConstants.DuplicatedOperation, duplicate.Timestamp, order.Timestamp);
                businessErrors.Add(DomainConstants.DuplicatedOperation);
            }

            // Closed market
            var openMarket = new TimeSpan(_settings.Value.MaketOpensAt, 0, 0);
            var closeMarket = new TimeSpan(_settings.Value.MarketClosesAt, 0, 0);
            if (order.Timestamp.TimeOfDay < openMarket || order.Timestamp.TimeOfDay > closeMarket)
            {
                _logger.LogDebug("{msg} Order:{order}", DomainConstants.ClosedMarket, order.Timestamp);
                businessErrors.Add(DomainConstants.ClosedMarket);
            }

            if (businessErrors.Any())
            {
                _logger.LogDebug("{errCount} business errors found while creating the Order.", businessErrors.Count);
            }
            else
            {
                account.Cash += order.TotalOrderAmount * (order.Operation == OperationType.Buy ? -1 : 1);
                account.Orders.Add(order);
                await _repository.UpdateAsync(account);
                _logger.LogDebug("Order successfully created for account {accountId}.", account.Id);
            }

            return new CurrentBalanceDTO
            {
                Cash = account.Cash,
                Issuers = account.Orders.Select(
                    o => new IssuerDTO
                    {
                        IssuerName = o.IssuerName,
                        TotalShares = o.TotalShares,
                        SharePrice = o.SharePrice
                    }
                ),
                BusinessErrors = businessErrors
            };
        }
    }
}