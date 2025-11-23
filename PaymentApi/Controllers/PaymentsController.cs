using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.Services;
using System.ComponentModel.DataAnnotations;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _svc;
        public PaymentsController(IPaymentService svc) { _svc = svc; }

        public record CreatePaymentDto([Required] decimal amount, [Required] string currency, [Required] string clientRequestId, string? provider);
        public record UpdatePaymentDto([Required] decimal amount, [Required] string currency);

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreatePaymentDto dto)
        {
            try
            {
                var payment = await _svc.CreateAsync(dto.amount, dto.currency, dto.clientRequestId, dto.provider);
                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List() => Ok(await _svc.ListAsync());

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _svc.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentDto dto)
        {
            try
            {
                var updated = await _svc.UpdateAsync(id, dto.amount, dto.currency);
                if (updated == null) return NotFound();
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
