using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<List<Curso>>> GetCurso()
        {
            var lista = await _context.Curso.ToListAsync();
            return Ok(lista);
        }


        [HttpGet]
        [Route("{id_curso}")]
        public async Task<ActionResult<List<Curso>>> GetSingleCurso(int id_curso)
        {
            var miobjeto = await _context.Curso.FirstOrDefaultAsync(ob => ob.id_curso == id_curso);
            if (miobjeto == null)
            {
                return NotFound(" :/");
            }

            return Ok(miobjeto);
        }

        [HttpPost]
        public async Task<ActionResult<Curso>> CreateCurso(Curso objeto)
        {

            _context.Curso.Add(objeto);
            await _context.SaveChangesAsync();
            return Ok(await GetDbCurso());
        }

        [HttpPut("{id_curso}")]
        public async Task<ActionResult<List<Curso>>> UpdateCurso(Curso objeto)
        {

            var DbObjeto = await _context.Curso.FindAsync(objeto.id_curso);
            if (DbObjeto == null)
                return BadRequest("no se encuentra");
            DbObjeto.nombre_curso = objeto.nombre_curso;


            await _context.SaveChangesAsync();

            return Ok(await _context.Curso.ToListAsync());


        }


        [HttpDelete]
        [Route("{id_curso}")]
        public async Task<ActionResult<List<Curso>>> DeleteCurso(int id_curso)
        {
            var DbObjeto = await _context.Curso.FirstOrDefaultAsync(Ob => Ob.id_curso == id_curso);
            if (DbObjeto == null)
            {
                return NotFound("no existe :/");
            }

            _context.Curso.Remove(DbObjeto);
            await _context.SaveChangesAsync();

            return Ok(await GetDbCurso());
        }


        private async Task<List<Curso>> GetDbCurso()
        {
            return await _context.Curso.ToListAsync();
        }

        [HttpGet]
        [Route("CursoByName/{nombre_curso}")]
        public async Task<ActionResult<List<Curso>>> GetSingleCurso(string nombre_curso)
        {
            var cursos = await _context.Curso
                .Where(c => c.nombre_curso.Contains(nombre_curso))
                .ToListAsync();

            if (cursos == null || cursos.Count == 0)
            {
                return NotFound("No se encontraron coincidencias :/");
            }

            return Ok(cursos);
        }

    }
}
