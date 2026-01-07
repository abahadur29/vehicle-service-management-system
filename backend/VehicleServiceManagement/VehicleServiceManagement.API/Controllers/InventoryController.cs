using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleServiceManagement.API.Application.Features.Inventory;

namespace VehicleServiceManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InventoryController(IMediator mediator) => _mediator = mediator;

        [HttpGet("parts")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> GetParts()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var result = await _mediator.Send(new GetAllPartsQuery(userRole));
            return Ok(result);
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetLowStock()
        {
            var result = await _mediator.Send(new GetLowStockQuery());
            return Ok(result);
        }
    }
}