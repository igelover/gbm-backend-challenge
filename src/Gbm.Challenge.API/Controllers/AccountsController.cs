using Gbm.Challenge.API.Models.Requests;
using Gbm.Challenge.Application.Exceptions;
using Gbm.Challenge.Application.Features.Accounts.Commands;
using Gbm.Challenge.Domain.Models.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using ValidationException = Gbm.Challenge.Application.Exceptions.ValidationException;

namespace Gbm.Challenge.API.Controllers
{
    /// <summary>
    /// Investment accounts and orders management
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize("ChallengeApiUser")]
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
            [Required][FromBody] CreateAccountRequest request
        )
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
                return BadRequest(string.Join(' ', ex.Errors.SelectMany(e => e.Value)));
            }
        }

        /// <summary>
        /// Creates a new order for the specified account
        /// </summary>
        /// <param name="id">The account ID</param>
        /// <param name="order">The order details</param>
        /// <returns>The account current balance and order details</returns>
        [HttpPost("{id}/orders")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateOrder(
            [Required][FromRoute] int id,
            [Required][FromBody] OrderDTO order
        )
        {
            try
            {
                var command = new CreateOrderCommand(id, order);
                var result = await _mediator.Send(command);
                _logger.LogDebug("Successfully created new order on account {id}", id);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(string.Join(' ', ex.Errors.SelectMany(e => e.Value)));
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}