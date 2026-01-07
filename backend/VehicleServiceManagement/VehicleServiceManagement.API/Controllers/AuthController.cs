using MediatR;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.Application.DTOs.Auth;
using VehicleServiceManagement.API.Application.Features.Auth;

namespace VehicleServiceManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(new RegisterUserCommand(dto));

            if (result)
                return Ok(new { message = "Registration successful" });

            return BadRequest(new { message = "Registration failed. Email might already be taken." });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mediator.Send(new LoginUserCommand(dto));

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(new { message = result.Message ?? "Invalid email or password." });
        }
    }
}