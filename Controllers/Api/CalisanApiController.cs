using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBERBERODEV.DATA;
using WEBBERBERODEV.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace WEBBERBERODEV.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Role_Admin)]
    public class CalisanApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CalisanApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CalisanApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calisan>>> GetCalisanlar()
        {
            return await _context.Calisanlar.ToListAsync();
        }

        // GET: api/CalisanApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Calisan>> GetCalisan(int id)
        {
            var calisan = await _context.Calisanlar.FindAsync(id);

            if (calisan == null)
            {
                return NotFound();
            }

            return calisan;
        }

        // POST: api/CalisanApi
        [HttpPost]
        public async Task<ActionResult<Calisan>> PostCalisan(Calisan calisan)
        {
            _context.Calisanlar.Add(calisan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalisan), new { id = calisan.Id }, calisan);
        }

        // PUT: api/CalisanApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCalisan(int id, Calisan calisan)
        {
            if (id != calisan.Id)
            {
                return BadRequest();
            }

            _context.Entry(calisan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Calisanlar.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/CalisanApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalisan(int id)
        {
            var calisan = await _context.Calisanlar.FindAsync(id);
            if (calisan == null)
            {
                return NotFound();
            }

            _context.Calisanlar.Remove(calisan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
