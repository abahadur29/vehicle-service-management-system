using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VehicleServiceManagement.API.Application.DTOs.Services;
using VehicleServiceManagement.API.Application.Features.Services;

namespace VehicleServiceManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetServiceNotificationsQuery(userId));
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyBookingsQuery(userId));
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> BookService([FromBody] CreateServiceRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var command = new CreateServiceRequestCommand(
                dto.VehicleId,
                dto.ServiceCategoryId,
                dto.Description ?? string.Empty,
                !string.IsNullOrEmpty(dto.Priority) ? dto.Priority : "Normal",
                dto.RequestedDate,
                userId!
            );

            var result = await _mediator.Send(command);
            return Ok(new { requestId = result.Id, message = "Service booked successfully" });
        }

        [HttpPost("assign")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> AssignTechnician([FromBody] UpdateServiceStatusCommand command)
        {
            var result = await _mediator.Send(command);
            return result ? Ok(new { message = "Status successfully updated" }) : BadRequest();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var result = await _mediator.Send(new CancelBookingCommand(id));
            return result ? Ok(new { message = "Service booking cancelled successfully" }) : BadRequest("Cannot cancel this service. Service must be in 'Requested' status with no technician assigned.");
        }

        [HttpPut("{id}/reschedule")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> RescheduleService(int id, [FromBody] RescheduleServiceRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new RescheduleServiceRequestCommand(id, dto.NewRequestedDate, userId));
            return result ? Ok(new { message = "Service rescheduled successfully" }) : BadRequest("Cannot reschedule. Service must be in 'Requested' status and belong to you.");
        }

        [HttpPost("complete")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> CompleteService([FromBody] CompleteServiceCommand command)
        {
            var result = await _mediator.Send(command);
            return result ? Ok(new { message = "Service completed" }) : BadRequest("Failed to complete service.");
        }

        [HttpGet("all")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetAllRequests()
        {
            var result = await _mediator.Send(new GetAllServiceRequestsQuery());
            return Ok(result);
        }

        [HttpGet("tasks")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> GetMyTasks([FromQuery] string? technicianId)
        {
            var idToUse = technicianId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idToUse)) return Unauthorized();

            var result = await _mediator.Send(new GetTechnicianTasksQuery(idToUse));
            return Ok(result);
        }

        [HttpGet("history")]
        [Authorize(Roles = "Technician,Admin")]
        public async Task<IActionResult> GetTechnicianServiceHistory([FromQuery] string? technicianId)
        {
            var idToUse = technicianId ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idToUse)) return Unauthorized();

            var result = await _mediator.Send(new GetTechnicianServiceHistoryQuery(idToUse));
            return Ok(result);
        }
    }
}