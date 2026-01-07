using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.Application.Features.Admin;
using VehicleServiceManagement.API.Application.Features.Reports;

namespace VehicleServiceManagement.API.Controllers
{
    [Authorize(Roles = "Manager")]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("technician-workload")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetTechnicianWorkload()
        {
            var result = await _mediator.Send(new GetTechnicianWorkloadQuery());
            return Ok(result);
        }

        [HttpGet("revenue-report")]
        public async Task<IActionResult> GetRevenueReport()
        {
            var result = await _mediator.Send(new GetRevenueReportQuery());
            return Ok(result);
        }

        [HttpGet("monthly-stats")]
        public async Task<IActionResult> GetMonthlyStats()
        {
            var result = await _mediator.Send(new GetMonthlyStatsQuery());
            return Ok(result);
        }

        [HttpGet("by-priority/{priority}")]
        public async Task<IActionResult> GetByPriority(string priority)
        {
            var result = await _mediator.Send(new GetByPriorityQuery(priority));
            return Ok(result);
        }

        [HttpGet("tech-performance")]
        public async Task<IActionResult> GetTechPerformance()
        {
            var result = await _mediator.Send(new GetTechPerformanceQuery());
            return Ok(result);
        }

        [HttpGet("revenue-by-type")]
        public async Task<IActionResult> GetRevenueByServiceType()
        {
            var result = await _mediator.Send(new GetServiceTypeRevenueQuery());
            return Ok(result);
        }

        [HttpGet("pending-vs-completed")]
        public async Task<IActionResult> GetPendingVsCompleted()
        {
            var result = await _mediator.Send(new GetPendingVsCompletedQuery());
            return Ok(result);
        }

        [HttpGet("vehicle-service-history")]
        public async Task<IActionResult> GetVehicleServiceHistoryReport()
        {
            var result = await _mediator.Send(new GetVehicleServiceHistoryReportQuery());
            return Ok(result);
        }
    }
}