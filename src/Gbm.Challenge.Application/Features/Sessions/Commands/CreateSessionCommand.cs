using FluentValidation;
using Gbm.Challenge.Application.Contracts.Identity;
using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Application.Exceptions;
using Gbm.Challenge.Domain.Common;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Identity;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Gbm.Challenge.Application.Features.Sessions.Commands;

public class CreateSessionCommand : IRequest<string>
{
    public string ClientName { get; set; }
    public string ApiKey { get; set; }

    public CreateSessionCommand(string clientName, string apiKey)
    {
        ClientName = clientName;
        ApiKey = apiKey;
    }

    public class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
    {
        public CreateSessionCommandValidator()
        {
            RuleFor(cmd => cmd.ClientName).NotEmpty().WithMessage("Client name is required.");
            RuleFor(cmd => cmd.ApiKey).NotEmpty().WithMessage("API key is required.");
        }
    }

    public class CreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, string>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IJwtService _jwtService;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<CreateSessionCommandHandler> _logger;

        public CreateSessionCommandHandler(
            IClientRepository clientRepository,
            IJwtService jwtService,
            ISessionRepository sessionRepository,
            ILogger<CreateSessionCommandHandler> logger)
        {
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetByNameAndKey(request.ClientName, request.ApiKey);
            if (client is null)
            {
                _logger.LogDebug("There is not a matching client {clientName} for the API key provided.", request.ClientName);
                throw new UnauthorizedException();
            }

            var (tokenId, jwtToken) = _jwtService.GenerateJwtFor(request.ClientName, DomainConstants.JwtScopeUserRole);
            var session = new Session()
            {
                Client = client,
                ClientName = client.Name,
                TokenId = tokenId
            };
            await _sessionRepository.CreateSessionAsync(client, session);
            _logger.LogDebug("A new session with tokenId {tokenId} has been created for client {client}", request.ClientName, tokenId);

            return jwtToken;
        }
    }
}