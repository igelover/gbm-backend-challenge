using Gbm.Challenge.API.Models.Requests;
using Gbm.Challenge.Application.Features.Accounts.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ValidationException = Gbm.Challenge.Application.Exceptions.ValidationException;

namespace Gbm.Challenge.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(IMediator mediator, ILogger<AccountsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a new Investment Account.
        /// </summary>
        /// <param name="request">A json object containing the initial cash.</param>
        /// <returns>The newly created account info</returns>
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAccount(
            [Required][FromBody] CreateAccountRequest request)
        {
            try
            {
                var command = new CreateAccountCommand(request.Cash);
                var result = await _mediator.Send(command);
                _logger.LogDebug("Successfully created account {id}", result.Id);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(string.Join(',', ex.Errors.SelectMany(e => e.Value)));
            }
        }
    }
}