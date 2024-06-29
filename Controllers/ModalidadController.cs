using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using LudoLab_ConnectSys_Server.Data;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModalidadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ModalidadController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Modalidad
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Modalidad>>> GetModalidades()
        {
            return await _context.Modalidad.ToListAsync();
        }

        // GET: api/Modalidad/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Modalidad>> GetModalidad(int id)
        {
            var modalidad = await _context.Modalidad.FindAsync(id);

            if (modalidad == null)
            {
                return NotFound();
            }

            return modalidad;
        }

        // POST: api/Modalidad
        [HttpPost]
        public async Task<ActionResult<Modalidad>> PostModalidad(Modalidad modalidad)
        {
            _context.Modalidad.Add(modalidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetModalidad), new { id = modalidad.id_modalidad }, modalidad);
        }

        // PUT: api/Modalidad/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutModalidad(int id, Modalidad modalidad)
        {
            if (id != modalidad.id_modalidad)
            {
                return BadRequest();
            }

            _context.Entry(modalidad).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModalidadExists(id))
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

        // DELETE: api/Modalidad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModalidad(int id)
        {
            var modalidad = await _context.Modalidad.FindAsync(id);
            if (modalidad == null)
            {
                return NotFound();
            }

            _context.Modalidad.Remove(modalidad);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ModalidadExists(int id)
        {
            return _context.Modalidad.Any(e => e.id_modalidad == id);
        }
    }
}
