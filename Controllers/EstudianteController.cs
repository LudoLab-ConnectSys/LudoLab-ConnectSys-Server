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
    public class EstudianteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EstudianteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Estudiante
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estudiante>>> GetEstudiantes()
        {
            return await _context.Estudiante.ToListAsync();
        }

        // GET: api/Estudiante/{id_estudiante}
        [HttpGet("{id_estudiante}")]
        public async Task<ActionResult<Estudiante>> GetEstudiante(int id_estudiante)
        {
            var estudiante = await _context.Estudiante.FindAsync(id_estudiante);

            if (estudiante == null)
            {
                return NotFound();
            }

            return estudiante;
        }

        // POST: api/Estudiante
        [HttpPost]
        public async Task<ActionResult<Estudiante>> PostEstudiante(Estudiante estudiante)
        {
            _context.Estudiante.Add(estudiante);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEstudiante", new { id_estudiante = estudiante.id_estudiante }, estudiante);
        }

        // PUT: api/Estudiante/{id_estudiante}
        [HttpPut("{id_estudiante}")]
        public async Task<IActionResult> PutEstudiante(int id_estudiante, Estudiante estudiante)
        {
            if (id_estudiante != estudiante.id_estudiante)
            {
                return BadRequest();
            }

            _context.Entry(estudiante).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstudianteExists(id_estudiante))
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

        // DELETE: api/Estudiante/{id_estudiante}
        [HttpDelete("{id_estudiante}")]
        public async Task<IActionResult> DeleteEstudiante(int id_estudiante)
        {
            var estudiante = await _context.Estudiante.FindAsync(id_estudiante);
            if (estudiante == null)
            {
                return NotFound();
            }

            _context.Estudiante.Remove(estudiante);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EstudianteExists(int id_estudiante)
        {
            return _context.Estudiante.Any(e => e.id_estudiante == id_estudiante);
        }
    }
}
