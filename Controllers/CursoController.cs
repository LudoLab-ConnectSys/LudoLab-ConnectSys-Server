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
    public class CursoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CursoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Curso>>> GetCursos()
        {
            return await _context.Curso.ToListAsync();
        }

        [HttpGet("{id_curso}")]
        public async Task<ActionResult<Curso>> GetCurso(int id_curso)
        {
            var curso = await _context.Curso.FirstOrDefaultAsync(c => c.id_curso == id_curso);
            if (curso == null)
            {
                return NotFound();
            }
            return curso;
        }

        [HttpPost]
        public async Task<ActionResult<Curso>> CreateCurso(Curso curso)
        {
            _context.Curso.Add(curso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCurso), new { id_curso = curso.id_curso }, curso);
        }

        [HttpPut("{id_curso}")]
        public async Task<IActionResult> UpdateCurso(int id_curso, Curso curso)
        {
            if (id_curso != curso.id_curso)
            {
                return BadRequest();
            }

            _context.Entry(curso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(id_curso))
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

        [HttpDelete("{id_curso}")]
        public async Task<IActionResult> DeleteCurso(int id_curso)
        {
            var curso = await _context.Curso.FindAsync(id_curso);
            if (curso == null)
            {
                return NotFound();
            }

            _context.Curso.Remove(curso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CursoExists(int id_curso)
        {
            return _context.Curso.Any(e => e.id_curso == id_curso);
        }
    }
}
