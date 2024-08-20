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

        [HttpGet("detalles/{id_periodo}")]
        public async Task<ActionResult<PeriodoConDetalles>> GetPeriodoDetalles(int id_periodo)
        {
            var periodo = await _context.Periodo
                .Where(p => p.id_periodo == id_periodo)
                .Select(p => new PeriodoConDetalles
                {
                    id_periodo = p.id_periodo,
                    id_curso = p.id_curso,
                    nombre_periodo = (from lp in _context.ListaPeriodo where lp.id_lista_periodo == p.id_ListaPeriodo select lp.nombre_periodo).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (periodo == null)
            {
                return NotFound();
            }

            return Ok(periodo);
        }

        /*-------- DETALLES CON CURSO --------*/

        [HttpGet("detalles-con-curso/{id_periodo}")]
        public async Task<ActionResult<PeriodoConNombreCurso>> GetPeriodoConCursoDetalles(int id_periodo)
        {
            var periodoConCurso = await _context.Periodo
                .Where(p => p.id_periodo == id_periodo)
                .Select(p => new PeriodoConNombreCurso
                {
                    id_periodo = p.id_periodo,
                    nombre_periodo = (from lp in _context.ListaPeriodo where lp.id_lista_periodo == p.id_ListaPeriodo select lp.nombre_periodo).FirstOrDefault(),
                    nombre_curso = (from c in _context.Curso where c.id_curso == p.id_curso select c.nombre_curso).FirstOrDefault(),
                    fecha_inicio_periodo = p.fecha_inicio_periodo,
                    fecha_fin_periodo = p.fecha_fin_periodo,
                    duracion_periodo_horas = p.duracion_periodo_horas
                })
                .FirstOrDefaultAsync();

            if (periodoConCurso == null)
            {
                return NotFound();
            }

            return Ok(periodoConCurso);
        }

        [HttpGet("Curso/{id_curso}")]
        public async Task<ActionResult<List<PeriodoConNombreCurso>>> GetPeriodosByCurso(int id_curso)
        {
            var query = from p in _context.Periodo
                        join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo
                        where p.id_curso == id_curso
                        select new PeriodoConNombreCurso
                        {
                            id_periodo = p.id_periodo,
                            nombre_periodo = lp.nombre_periodo,
                            fecha_inicio_periodo = p.fecha_inicio_periodo,
                            fecha_fin_periodo = p.fecha_fin_periodo,
                            duracion_periodo_horas = p.duracion_periodo_horas,
                            id_ListaPeriodo = p.id_ListaPeriodo,
                            nombre_curso = (from c in _context.Curso where c.id_curso == p.id_curso select c.nombre_curso).FirstOrDefault(),
                            nombre_certificado = (from cert in _context.Certificado where cert.id_periodo == p.id_periodo select cert.nombre_certificado).FirstOrDefault()
                        };

            var lista = await query.ToListAsync();

            return Ok(lista);
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



        private bool PeriodoExists(int id_periodo)
        {
            return _context.Periodo.Any(e => e.id_periodo == id_periodo);
        }

        [HttpGet]
        [Route("PeriodoByName")]
        public async Task<ActionResult<PagedResult<PeriodoConNombreCurso>>> GetSinglePeriodo([FromQuery] int id_lista_periodo, [FromQuery] string nombre_curso = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 3)
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

            // Obtener el total de registros
            var totalRecords = await query.CountAsync();

            // Calcular el total de páginas
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Aplicar paginación
            var items = await query
                .OrderByDescending(p => p.id_periodo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedResult = new PagedResult<PeriodoConNombreCurso>
            {
                Items = items,
                TotalPages = totalPages,
                CurrentPage = pageNumber
            };

            return Ok(pagedResult);
        }




        [HttpGet]
        [Route("getNamePeriodoList")]
        public async Task<ActionResult<List<ListaPeriodo>>> GetListaPeriodos()
        {
            var listaPeriodos = await _context.ListaPeriodo.ToListAsync();
            return Ok(listaPeriodos);
        }

        // Obtener cursos por periodo
        [HttpGet("{id_periodo}/cursos")]
        public async Task<ActionResult<List<Curso>>> GetCursosByPeriodo(int id_periodo)
        {
            var periodos = await _context.Periodo
                .Where(p => p.id_periodo == id_periodo)
                .ToListAsync();

            if (periodos == null || !periodos.Any())
            {
                return NotFound("No se encontraron cursos para el periodo especificado.");
            }

            var cursos = await _context.Curso
                .Where(c => periodos.Select(p => p.id_curso).Contains(c.id_curso))
                .ToListAsync();

            return Ok(cursos);
        }

        // Obtener todos los periodos con nombres
        [HttpGet("all-periodos-con-nombre")]
        public async Task<ActionResult<List<PeriodoConNombre>>> GetPeriodos()
        {
            var periodos = await _context.Periodo.ToListAsync();
            var listaPeriodos = await _context.ListaPeriodo.ToListAsync();

            var periodosConNombre = periodos.Select(p => new PeriodoConNombre
            {
                id_periodo = p.id_periodo,
                fecha_inicio_periodo = p.fecha_inicio_periodo,
                fecha_fin_periodo = p.fecha_fin_periodo,
                duracion_periodo_horas = p.duracion_periodo_horas,
                id_curso = p.id_curso,
                id_ListaPeriodo = p.id_ListaPeriodo,
                nombre_periodo = listaPeriodos.FirstOrDefault(lp => lp.id_lista_periodo == p.id_ListaPeriodo)?.nombre_periodo
            }).ToList();

            return Ok(periodosConNombre);
        }

        [HttpGet("por-curso/{id_curso}")]
        public async Task<ActionResult<List<PeriodoConNombre>>> GetPeriodosPorCurso(int id_curso)
        {
            var periodos = await _context.Periodo
                .Where(p => p.id_curso == id_curso)
                .ToListAsync();

            var listaPeriodos = await _context.ListaPeriodo.ToListAsync();
            var periodosConNombre = periodos.Select(p => new PeriodoConNombre
            {
                id_periodo = p.id_periodo,
                fecha_inicio_periodo = p.fecha_inicio_periodo,
                fecha_fin_periodo = p.fecha_fin_periodo,
                duracion_periodo_horas = p.duracion_periodo_horas,
                id_curso = p.id_curso,
                id_ListaPeriodo = p.id_ListaPeriodo,
                nombre_periodo = listaPeriodos.FirstOrDefault(lp => lp.id_lista_periodo == p.id_ListaPeriodo)?.nombre_periodo
            }).ToList();

            if (!periodosConNombre.Any())
            {
                return NotFound($"No se encontraron periodos para el curso con id {id_curso}");
            }

            return Ok(periodosConNombre);
        }


        [HttpGet("Activos")]
        public async Task<ActionResult<List<PeriodoConCursoInfo>>> GetPeriodosActivos()
        {
            var query = from p in _context.Periodo
                        join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo
                        join c in _context.Curso on p.id_curso equals c.id_curso
                        where p.activo == true
                        select new PeriodoConCursoInfo
                        {
                            id_periodo = p.id_periodo,
                            nombre_periodo = lp.nombre_periodo,
                            fecha_inicio_periodo = p.fecha_inicio_periodo,
                            fecha_fin_periodo = p.fecha_fin_periodo,
                            duracion_periodo_horas = p.duracion_periodo_horas,
                            nombre_curso = c.nombre_curso,
                            id_curso = p.id_curso // Asegúrate de que esto está siendo asignado
                        };

            var lista = await query.ToListAsync();
            return Ok(lista);
        }



    }
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }


}
