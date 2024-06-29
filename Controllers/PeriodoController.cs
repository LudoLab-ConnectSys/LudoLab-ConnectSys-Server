using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeriodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PeriodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Periodo>>> GetPeriodos()
        {
            return await _context.Periodo.ToListAsync();
        }

        [HttpGet("{id_periodo}")]
        public async Task<ActionResult<Periodo>> GetPeriodo(int id_periodo)
        {
            var periodo = await _context.Periodo.FirstOrDefaultAsync(p => p.id_periodo == id_periodo);
            if (periodo == null)
            {
                return NotFound();
            }
            return periodo;
        }

        [HttpGet("Curso/{id_curso}")]
        public async Task<ActionResult<List<Periodo>>> GetPeriodosByCurso(int id_curso)
        {
            var periodos = await _context.Periodo.Where(p => p.id_curso == id_curso).ToListAsync();
            return periodos;
        }

        [HttpPost]
        public async Task<ActionResult<Periodo>> CreatePeriodo(Periodo periodo)
        {
            _context.Periodo.Add(periodo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPeriodo), new { id_periodo = periodo.id_periodo }, periodo);
        }

        [HttpPut("{id_periodo}")]
        public async Task<IActionResult> UpdatePeriodo(int id_periodo, Periodo periodo)
        {
            if (id_periodo != periodo.id_periodo)
            {
                return BadRequest();
            }

            _context.Entry(periodo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PeriodoExists(id_periodo))
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

        [HttpDelete("{id_periodo}")]
        public async Task<IActionResult> DeletePeriodo(int id_periodo)
        {
            var periodo = await _context.Periodo.FindAsync(id_periodo);
            if (periodo == null)
            {
                return NotFound();
            }

            _context.Periodo.Remove(periodo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PeriodoExists(int id_periodo)
        {
            return _context.Periodo.Any(e => e.id_periodo == id_periodo);
        }
    }
}
