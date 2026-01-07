using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VehicleServiceManagement.API.Application.DTOs.Vehicles;
using VehicleServiceManagement.API.Application.Features.Reports;
using VehicleServiceManagement.API.Application.Features.Vehicles;

namespace VehicleServiceManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public VehiclesController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User identity not found.");
            }

            var command = new AddVehicleCommand(
                dto.LicensePlate,
                dto.Model,
                dto.Make,
                dto.Year,
                userId
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("my-vehicles")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyVehicles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetMyVehiclesQuery(userId));
            return Ok(result);
        }

        [HttpGet("{id}/history")]
        [Authorize(Roles = "Customer,Technician,Manager,Admin")]
        public async Task<IActionResult> GetVehicleHistory(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User identity not found.");
            }

            var result = await _mediator.Send(new GetVehicleServiceHistoryQuery(id, userId, userRole));

            if (result == null || result.Count == 0)
            {
                return NotFound("No service history found for this vehicle or you do not have access to view it.");
            }

            return Ok(result);
        }
    }
}