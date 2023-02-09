using FluentValidation;
using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Models.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gbm.Challenge.Application.Features.Accounts.Commands;

public class CreateAccountCommand : IRequest<AccountDTO>
{
    public decimal Cash { get; set; }

    public CreateAccountCommand(decimal cash)
    {
        Cash = cash;
    }

    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public CreateAccountCommandValidator()
        {
            RuleFor(cmd => cmd.Cash).GreaterThan(decimal.Zero).WithMessage("Initial cash must be greater than zero.");
        }
    }

    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDTO>
    {
        private readonly IAccountRepository _repository;
        private readonly ILogger<CreateAccountCommandHandler> _logger;

        public CreateAccountCommandHandler(IAccountRepository repository, ILogger<CreateAccountCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AccountDTO> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await _repository.AddAsync(new InvestmentAccount { Cash = request.Cash });
            _logger.LogDebug("Account {id} successfully created.", account.Id);

            return new AccountDTO
            {
                Id = account.Id,
                Cash = account.Cash,
            };
        }
    }
}