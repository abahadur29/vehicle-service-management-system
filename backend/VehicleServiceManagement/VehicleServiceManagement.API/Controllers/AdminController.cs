using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleServiceManagement.API.Application.Features.Admin;
using VehicleServiceManagement.API.Application.Features.Inventory;
using VehicleServiceManagement.API.Application.Features.Reports;
using VehicleServiceManagement.API.Application.Features.Services;
using VehicleServiceManagement.API.Application.Features.Users;
using VehicleServiceManagement.API.Application.DTOs.Admin;
using VehicleServiceManagement.API.Application.DTOs.Inventory;

namespace VehicleServiceManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region User & Role Management

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _mediator.Send(new GetAllUsersQuery());
            return Ok(result);
        }

        [HttpGet("technicians")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetTechnicians()
        {
            var result = await _mediator.Send(new GetAvailableTechniciansQuery());
            return Ok(result);
        }

        [HttpPut("update-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto dto)
        {
            var result = await _mediator.Send(new UpdateUserRoleCommand(dto.UserId, dto.NewRole));
            if (result.Success)
                return Ok(new { message = "Role updated successfully" });
            
            return BadRequest(new { message = result.ErrorMessage ?? "Failed to update role." });
        }

        [HttpPost("create-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var result = await _mediator.Send(new CreateUserCommand(dto));
            if (result.Success)
                return Ok(new { message = "User created successfully", userId = result.UserId });
            
            return BadRequest(new { message = result.ErrorMessage ?? "Failed to create user." });
        }

        [HttpDelete("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _mediator.Send(new DeleteUserCommand(userId));
            if (result.Success)
                return Ok(new { message = "User deleted successfully" });
            
            return BadRequest(new { message = result.ErrorMessage ?? "Failed to delete user." });
        }

        [HttpPost("toggle-user-active/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserActive(string userId)
        {
            var result = await _mediator.Send(new ToggleUserActiveCommand(userId));
            if (result.Success)
                return Ok(new { message = "User status updated successfully" });
            
            return BadRequest(new { message = result.ErrorMessage ?? "Failed to update user status." });
        }

        #endregion

        #region Service Approval & Workflow

        [HttpPost("approve-service/{id}")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> ApproveService(int id)
        {
            var result = await _mediator.Send(new ApproveServiceCommand(id));
            return result ? Ok(new { message = "Service processed successfully." })
                          : BadRequest("Service is not in a state that requires approval.");
        }

        #endregion

        #region Inventory Management

        [HttpGet("parts")]
        [Authorize(Roles = "Admin,Manager,Technician")]
        public async Task<IActionResult> GetAllParts()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var result = await _mediator.Send(new GetAllPartsQuery(userRole));
            return Ok(result);
        }

        [HttpPost("add-part")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPart([FromBody] CreatePartDto dto)
        {
            var result = await _mediator.Send(new AddPartCommand(dto));
            return Ok(result);
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetLowStock()
        {
            var result = await _mediator.Send(new GetLowStockQuery());
            return Ok(result);
        }

        [HttpPut("update-stock/{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            var result = await _mediator.Send(new UpdateStockCommand(id, dto.Quantity));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update-price/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdatePriceDto dto)
        {
            var result = await _mediator.Send(new UpdatePriceCommand(id, dto.NewPrice));
            return result ? Ok(new { message = "Price updated successfully" }) : BadRequest(new { message = "Failed to update price" });
        }

        [HttpPost("toggle-active/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _mediator.Send(new ToggleActiveCommand(id));
            return result ? Ok(new { message = "Part status updated successfully" }) : BadRequest(new { message = "Failed to update part status" });
        }

        #endregion

        #region Service Category & Pricing Management

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories([FromQuery] bool activeOnly = false)
        {
            var result = await _mediator.Send(new GetCategoriesQuery(activeOnly));
            return Ok(result);
        }

        [HttpPost("manage-category")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageCategory([FromBody] ManageCategoryDto dto)
        {
            var result = await _mediator.Send(new ManageCategoryCommand(dto));
            return Ok(result);
        }

        #endregion

        #region Reporting & Dashboards

        [HttpGet("revenue-report")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueReport()
        {
            var result = await _mediator.Send(new GetRevenueReportQuery());
            return Ok(result);
        }

        [HttpGet("monthly-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMonthlyStats()
        {
            var result = await _mediator.Send(new GetMonthlyStatsQuery());
            return Ok(result);
        }

        [HttpGet("revenue-by-type")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueByServiceType()
        {
            var result = await _mediator.Send(new GetServiceTypeRevenueQuery());
            return Ok(result);
        }

        [HttpGet("user-role-distribution")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserRoleDistribution()
        {
            var result = await _mediator.Send(new GetUserRoleDistributionQuery());
            return Ok(result);
        }

        #endregion
    }
}