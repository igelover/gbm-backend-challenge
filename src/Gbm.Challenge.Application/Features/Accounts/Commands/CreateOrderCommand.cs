using AutoMapper;
using FluentValidation;
using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Application.Exceptions;
using Gbm.Challenge.Domain.Common;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Models.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            IAccountRepository repository,
            IMapper mapper,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CurrentBalanceDTO> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var order = _mapper.Map<Order>(request.Order);
            var account = await _repository.GetByIdAsync(request.AccountId, true);
            if (account is null)
            {
                throw new NotFoundException(nameof(InvestmentAccount), request.AccountId);
            }

            // Business rules validations
            List<string> businessErrors = new();

            // Insufficient balance
            var totalOrderAmount = order.TotalOrderAmount;
            if (order.Operation == OperationType.Buy && totalOrderAmount > account.Cash)
            {
                businessErrors.Add(DomainConstants.InsufficientBalance);
            }

            // Insufficient stocks
            if (order.Operation == OperationType.Sell && account.GetStocksCount(order.IssuerName) < order.TotalShares)
            {
                businessErrors.Add(DomainConstants.InsufficientStocks);
            }

            // Duplicated operation
            if (account.Orders.Any(
                o => o.Operation == order.Operation
                  && o.IssuerName == order.IssuerName
                  && o.SharePrice == order.SharePrice
                  && Math.Abs((o.Timestamp - order.Timestamp).TotalMinutes) < 5))
            {
                businessErrors.Add(DomainConstants.DuplicatedOperation);
            }

            // Closed market
            var openMarket = new TimeSpan(6, 0, 0);
            var closeMarket = new TimeSpan(15, 0, 0);
            if (order.Timestamp.TimeOfDay < openMarket || order.Timestamp.TimeOfDay > closeMarket)
            {
                businessErrors.Add(DomainConstants.ClosedMarket);
            }

            if (businessErrors.Any())
            {
                _logger.LogInformation("{errCount} business errors found while creating the Order.", businessErrors.Count);
            }
            else
            {
                account.Cash += order.TotalOrderAmount * (order.Operation == OperationType.Buy ? -1 : 1);
                account.Orders.Add(order);
                await _repository.UpdateAsync(account);
                _logger.LogInformation("Order successfully created for account {accountId}.", account.Id);
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