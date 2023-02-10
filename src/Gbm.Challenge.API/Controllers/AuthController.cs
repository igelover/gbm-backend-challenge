using Gbm.Challenge.API.Models.Requests;
using Gbm.Challenge.Application.Exceptions;
using Gbm.Challenge.Application.Features.Sessions.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ValidationException = Gbm.Challenge.Application.Exceptions.ValidationException;

namespace Gbm.Challenge.API.Controllers
{
    /// <summary>
    /// Client authentication
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize("ChallengeApiUser")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a client with their API key
        /// </summary>
        /// <param name="request">The authentication request containing client name and API key</param>
        /// <returns>A session token</returns>
        [HttpPost()]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSession(
            [Required][FromBody] AuthenticationRequest request
        )
        {
            try
            {
                var command = new CreateSessionCommand(request.ClientName, request.ApiKey);
                var result = await _mediator.Send(command);
                _logger.LogDebug("Successfully created session with token {token}", result);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(string.Join(' ', ex.Errors.SelectMany(e => e.Value)));
            }
            catch (UnauthorizedException)
            {
                return Unauthorized();
            }
        }
    }
}