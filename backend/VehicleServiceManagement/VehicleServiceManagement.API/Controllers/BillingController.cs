using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleServiceManagement.API.Application.Features.Invoices;

namespace VehicleServiceManagement.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BillingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> GetMyInvoices()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyInvoicesQuery(userId));
            return Ok(result);
        }

        [HttpGet("invoice/{id}")]
        [Authorize(Roles = "Customer,Technician,Manager,Admin")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new GetInvoiceByServiceQuery(id, userId, userRole));
            
            if (result == null) 
                return NotFound("Invoice not found or you do not have access to this invoice.");
            
            return Ok(result);
        }

        [HttpPost("pay/{invoiceId}")]
        [Authorize(Roles = "Customer,Admin")]
        public async Task<IActionResult> PayInvoice(int invoiceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _mediator.Send(new ProcessPaymentCommand(invoiceId, userId, userRole));
            
            if (result.Success)
                return Ok(new { message = "Payment successful" });
            
            return BadRequest(new { message = result.ErrorMessage ?? "Payment failed." });
        }
    }
}