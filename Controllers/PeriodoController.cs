using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<List<PeriodoConNombreCurso>>> GetPeriodo()
        {
            var query = from p in _context.Periodo
                        join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo
                        join c in _context.Curso on p.id_curso equals c.id_curso
                        join cert in _context.Certificado on p.id_periodo equals cert.id_periodo
                        select new PeriodoConNombreCurso
                        {
                            id_periodo = p.id_periodo,
                            nombre_periodo = lp.nombre_periodo,
                            fecha_inicio_periodo = p.fecha_inicio_periodo,
                            fecha_fin_periodo = p.fecha_fin_periodo,
                            duracion_periodo_horas = p.duracion_periodo_horas,
                            nombre_curso = c.nombre_curso,
                            nombre_certificado = cert.nombre_certificado
                        };

            // Aplicar filtro si se proporciona
            /*if (!string.IsNullOrEmpty(filtro))
            {
                query = query.Where(p => p.nombre_periodo.Contains(filtro) ||
                                         p.nombre_curso.Contains(filtro) ||
                                         p.nombre_certificado.Contains(filtro));
            }*/

            var lista = await query.ToListAsync();

            lista.Reverse(); // Revierte el orden de la lista si es necesario
            return Ok(lista);
        }


        [HttpGet]
        [Route("{id_periodo}")]
        public async Task<ActionResult<List<Periodo>>> GetSinglePeriodo(int id_periodo)
        {
            var miobjeto = await _context.Periodo.FirstOrDefaultAsync(ob => ob.id_periodo == id_periodo);
            if (miobjeto == null)
            {
                return NotFound(" :/");
            }

            return Ok(miobjeto);
        }

        [HttpPost]
        public async Task<ActionResult<Periodo>> CreatePeriodo(Periodo objeto)
        {

            _context.Periodo.Add(objeto);
            await _context.SaveChangesAsync();
            return Ok(await GetDbPeriodo());
        }

        /*[HttpPut("{id_periodo}")]
        public async Task<ActionResult<List<Periodo>>> UpdatePeriodo(Periodo objeto)
        {

            var DbObjeto = await _context.Periodo.FindAsync(objeto.id_periodo);
            if (DbObjeto == null)
                return BadRequest("no se encuentra");
            DbObjeto.nombre_periodo = objeto.nombre_periodo;


            await _context.SaveChangesAsync();

            return Ok(await _context.Periodo.ToListAsync());


        }*/


        [HttpDelete]
        [Route("{id_periodo}")]
        public async Task<ActionResult<List<Periodo>>> DeletePeriodo(int id_periodo)
        {
            var DbObjeto = await _context.Periodo.FirstOrDefaultAsync(Ob => Ob.id_periodo == id_periodo);
            if (DbObjeto == null)
            {
                return NotFound("no existe :/");
            }

            _context.Periodo.Remove(DbObjeto);
            await _context.SaveChangesAsync();

            return Ok(await GetDbPeriodo());
        }


        private async Task<List<Periodo>> GetDbPeriodo()
        {
            return await _context.Periodo.ToListAsync();
        }

        [HttpGet]
        [Route("PeriodoByName")]
        public async Task<ActionResult<List<PeriodoConNombreCurso>>> GetSinglePeriodo([FromQuery] int id_lista_periodo, [FromQuery] string nombre_curso = null)
        {
            Console.WriteLine($"ID Lista Periodo: {id_lista_periodo}, Nombre Curso: {nombre_curso}");
            var query = from p in _context.Periodo
                        join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo
                        join c in _context.Curso on p.id_curso equals c.id_curso
                        join cert in _context.Certificado on p.id_periodo equals cert.id_periodo
                        select new PeriodoConNombreCurso
                        {
                            id_periodo = p.id_periodo,
                            id_ListaPeriodo = p.id_ListaPeriodo,
                            nombre_periodo = lp.nombre_periodo,
                            fecha_inicio_periodo = p.fecha_inicio_periodo,
                            fecha_fin_periodo = p.fecha_fin_periodo,
                            duracion_periodo_horas = p.duracion_periodo_horas,
                            nombre_curso = c.nombre_curso,
                            nombre_certificado = cert.nombre_certificado
                        };


            // Aplicar filtro por id_lista_periodo si es mayor que cero
            if (id_lista_periodo > 0)
            {
                query = query.Where(p => p.id_ListaPeriodo == id_lista_periodo);
            }

            // Aplicar filtro por nombre_curso si se proporciona
            if (!string.IsNullOrEmpty(nombre_curso))
            {
                query = query.Where(p => p.nombre_curso.Contains(nombre_curso));
            }

            var lista = await query.OrderByDescending(p => p.id_periodo).ToListAsync();

            if (lista == null || lista.Count == 0)
            {
                return NotFound("No se encontraron coincidencias :/");
            }

            return Ok(lista);
        }


        [HttpGet]
        [Route("getNamePeriodoList")]
        public async Task<ActionResult<List<ListaPeriodo>>> GetListaPeriodos()
        {
            var listaPeriodos = await _context.ListaPeriodo.ToListAsync();
            return Ok(listaPeriodos);
        }
    }
}
