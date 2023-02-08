using FluentValidation;
using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Application.Models.DTOs;
using Gbm.Challenge.Domain.Entities;
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

    public class AddAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        public AddAccountCommandValidator()
        {
            RuleFor(cmd => cmd.Cash).GreaterThan(decimal.Zero).WithMessage("Initial cash must be greater than zero.");
        }
    }

    public class AddAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDTO>
    {
        private readonly IAccountRepository _repository;
        private readonly ILogger<AddAccountCommandHandler> _logger;

        public AddAccountCommandHandler(IAccountRepository repository, ILogger<AddAccountCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AccountDTO> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await _repository.AddAsync(new InvestmentAccount { Cash = request.Cash });
            _logger.LogInformation("Account {id} successfully created.", account.Id);

            return new AccountDTO
            {
                Id = account.Id,
                Cash = account.Cash,
            };
        }
    }
}