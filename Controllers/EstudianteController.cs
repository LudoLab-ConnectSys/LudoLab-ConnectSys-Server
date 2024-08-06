using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using LudoLab_ConnectSys_Server.Data;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

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
        /*[HttpGet("Detalles")]
        public async Task<ActionResult<IEnumerable<EstudianteConDetalles>>> GetEstudiantesConDetalles()
        {
            var estudiantes = await _context.Estudiante
                .Join(_context.Usuario, e => e.id_usuario, u => u.id_usuario, (e, u) => new { e, u })
                .Join(_context.Grupo, eu => eu.e.id_grupo, g => g.id_grupo, (eu, g) => new { eu.e, eu.u, g })
                .Join(_context.Periodo, eg => eg.g.id_periodo, p => p.id_periodo, (eg, p) => new { eg.e, eg.u, eg.g, p })
                .Join(_context.Curso, pg => pg.p.id_curso, c => c.id_curso, (pg, c) => new { pg.e, pg.u, pg.g, pg.p, c })
                .Join(_context.ListaPeriodo, pc => pc.p.id_ListaPeriodo, lp => lp.id_lista_periodo, (pc, lp) => new { pc.e, pc.u, pc.g, pc.p, pc.c, lp })
                .GroupBy(x => new { x.e.id_estudiante, x.u.nombre_usuario, x.u.apellidos_usuario, x.u.edad_usuario, x.u.correo_usuario })
                .Select(group => new EstudianteConDetalles
                {
                    id_estudiante = group.Key.id_estudiante,
                    nombre_usuario = group.Key.nombre_usuario,
                    apellidos_usuario = group.Key.apellidos_usuario,
                    edad_usuario = group.Key.edad_usuario,
                    correo_usuario = group.Key.correo_usuario,
                    cursos = group.Select(g => g.c.nombre_curso).ToList(),
                    periodos = group.Select(g => g.lp.nombre_periodo).ToList(),
                    id_cursos = group.Select(g => g.c.id_curso).ToList(),
                    id_periodos = group.Select(g => g.lp.id_lista_periodo).ToList()
                })
                .ToListAsync();

            return Ok(estudiantes);
        }*/

        /*[HttpGet("Detalles")]
        public async Task<ActionResult<IEnumerable<EstudianteConDetalles>>> GetEstudiantesConDetalles(int pageNumber = 1, int pageSize = 10)
        {
            var estudiantesQuery = from e in _context.Estudiante
                                   join u in _context.Usuario on e.id_usuario equals u.id_usuario
                                   join g in _context.Grupo on e.id_grupo equals g.id_grupo into eg
                                   from g in eg.DefaultIfEmpty()
                                   join p in _context.Periodo on g.id_periodo equals p.id_periodo into pg
                                   from p in pg.DefaultIfEmpty()
                                   join c in _context.Curso on p.id_curso equals c.id_curso into pc
                                   from c in pc.DefaultIfEmpty()
                                   join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo into lpc
                                   from lp in lpc.DefaultIfEmpty()
                                   select new
                                   {
                                       e,
                                       u,
                                       g,
                                       p,
                                       c,
                                       lp
                                   };

            var estudiantesGrouped = estudiantesQuery
                .GroupBy(x => new { x.e.id_estudiante, x.u.nombre_usuario, x.u.apellidos_usuario, x.u.edad_usuario, x.u.correo_usuario, x.u.cedula_usuario })
                .Select(group => new EstudianteConDetalles
                {
                    id_estudiante = group.Key.id_estudiante,
                    nombre_usuario = group.Key.nombre_usuario,
                    apellidos_usuario = group.Key.apellidos_usuario,
                    edad_usuario = group.Key.edad_usuario,
                    correo_usuario = group.Key.correo_usuario,
                    cedula_usuario = group.Key.cedula_usuario,
                    cursos = group.Where(g => g.c != null).Select(g => g.c.nombre_curso).ToList(),
                    periodos = group.Where(g => g.lp != null).Select(g => g.lp.nombre_periodo).ToList(),
                    id_cursos = group.Where(g => g.c != null).Select(g => g.c.id_curso).ToList(),
                    id_periodos = group.Where(g => g.lp != null).Select(g => g.lp.id_lista_periodo).ToList()
                });

            var totalItems = await estudiantesGrouped.CountAsync();
            var estudiantesPaged = await estudiantesGrouped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalItems.ToString());

            return Ok(estudiantesPaged);
        }*/

        [HttpGet("Detalles")]
        public async Task<ActionResult<PagedResponse<EstudianteConDetalles>>> GetEstudiantesConDetalles(int pageNumber = 1, int pageSize = 10)
        {
            var estudiantesQuery = from e in _context.Estudiante
                                   join u in _context.Usuario on e.id_usuario equals u.id_usuario
                                   join g in _context.Grupo on e.id_grupo equals g.id_grupo into eg
                                   from g in eg.DefaultIfEmpty()
                                   join p in _context.Periodo on g.id_periodo equals p.id_periodo into pg
                                   from p in pg.DefaultIfEmpty()
                                   join c in _context.Curso on p.id_curso equals c.id_curso into pc
                                   from c in pc.DefaultIfEmpty()
                                   join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo into lpc
                                   from lp in lpc.DefaultIfEmpty()
                                   select new
                                   {
                                       e,
                                       u,
                                       g,
                                       p,
                                       c,
                                       lp
                                   };

            var estudiantesGrouped = estudiantesQuery
                .GroupBy(x => new { x.e.id_estudiante, x.u.nombre_usuario, x.u.apellidos_usuario, x.u.edad_usuario, x.u.correo_usuario, x.u.cedula_usuario })
                .Select(group => new EstudianteConDetalles
                {
                    id_estudiante = group.Key.id_estudiante,
                    nombre_usuario = group.Key.nombre_usuario,
                    apellidos_usuario = group.Key.apellidos_usuario,
                    edad_usuario = group.Key.edad_usuario,
                    correo_usuario = group.Key.correo_usuario,
                    cedula_usuario = group.Key.cedula_usuario,
                    cursos = group.Where(g => g.c != null).Select(g => g.c.nombre_curso).ToList(),
                    periodos = group.Where(g => g.lp != null).Select(g => g.lp.nombre_periodo).ToList(),
                    id_cursos = group.Where(g => g.c != null).Select(g => g.c.id_curso).ToList(),
                    id_periodos = group.Where(g => g.lp != null).Select(g => g.lp.id_lista_periodo).ToList()
                });

            var totalItems = await estudiantesGrouped.CountAsync();
            var estudiantesPaged = await estudiantesGrouped
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<EstudianteConDetalles>
            {
                Items = estudiantesPaged,
                TotalCount = totalItems
            };

            return Ok(response);
        }




        [HttpGet("DetallesEstudiante/{id_estudiante}")]
        public async Task<ActionResult<EstudianteConDetalles>> GetEstudianteConDetalles(int id_estudiante)
        {
            var estudianteConDetalles = await (from estudiante in _context.Estudiante
                                               join usuario in _context.Usuario on estudiante.id_usuario equals usuario.id_usuario
                                               join grupo in _context.Grupo on estudiante.id_grupo equals grupo.id_grupo into grp
                                               from grupo in grp.DefaultIfEmpty()
                                               join periodo in _context.Periodo on grupo.id_periodo equals periodo.id_periodo into prd
                                               from periodo in prd.DefaultIfEmpty()
                                               join listaPeriodo in _context.ListaPeriodo on periodo.id_ListaPeriodo equals listaPeriodo.id_lista_periodo into lpd
                                               from listaPeriodo in lpd.DefaultIfEmpty()
                                               join curso in _context.Curso on periodo.id_curso equals curso.id_curso into crs
                                               from curso in crs.DefaultIfEmpty()
                                               where estudiante.id_estudiante == id_estudiante
                                               select new EstudianteConDetalles
                                               {
                                                   id_estudiante = estudiante.id_estudiante,
                                                   nombre_usuario = usuario.nombre_usuario,
                                                   apellidos_usuario = usuario.apellidos_usuario,
                                                   cedula_usuario = usuario.cedula_usuario,
                                                   edad_usuario = usuario.edad_usuario,
                                                   correo_usuario = usuario.correo_usuario,
                                                   celular_usuario = usuario.celular_usuario,
                                                   telefono_usuario = usuario.telefono_usuario,
                                                   tipo_estudiante = estudiante.tipo_estudiante, // Nueva propiedad
                                                   nombre_grupo = grupo != null ? grupo.nombre_grupo : string.Empty,
                                                   nombre_curso = curso != null ? curso.nombre_curso : string.Empty,
                                                   nombre_periodo = listaPeriodo != null ? listaPeriodo.nombre_periodo : string.Empty,
                                                   id_lista_periodo = listaPeriodo != null ? listaPeriodo.id_lista_periodo : 0
                                               }).FirstOrDefaultAsync();

            if (estudianteConDetalles == null)
            {
                return NotFound();
            }

            return Ok(estudianteConDetalles);
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
        /* [HttpPut("{id_estudiante}")]
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
         }*/

        [HttpPut("{id_estudiante}")]
        public async Task<IActionResult> PutEstudiante(int id_estudiante, EstudianteConNombre estudianteConNombre)
        {
            if (id_estudiante != estudianteConNombre.id_estudiante)
            {
                return BadRequest();
            }

            var estudiante = await _context.Estudiante.FindAsync(id_estudiante);
            if (estudiante == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(estudiante.id_usuario);
            if (usuario == null)
            {
                return NotFound();
            }

            // Actualizar la información del usuario
            usuario.nombre_usuario = estudianteConNombre.nombre_usuario;
            usuario.apellidos_usuario = estudianteConNombre.apellidos_usuario;
            usuario.cedula_usuario = estudianteConNombre.cedula_usuario;
            usuario.edad_usuario = estudianteConNombre.edad_usuario;
            usuario.correo_usuario = estudianteConNombre.correo_usuario;
            usuario.celular_usuario = estudianteConNombre.celular_usuario;
            usuario.telefono_usuario = estudianteConNombre.telefono_usuario;

            // Actualizar la información del estudiante
            estudiante.tipo_estudiante = estudianteConNombre.tipo_estudiante;

            _context.Entry(usuario).State = EntityState.Modified;
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
            // Verificar si el estudiante existe
            var estudiante = await _context.Estudiante.FindAsync(id_estudiante);
            if (estudiante == null)
            {
                return NotFound(new { message = "Estudiante no encontrado" });
            }

            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync("EXEC sp_EliminarEstudiante @p0", id_estudiante);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al eliminar el estudiante: {ex.Message}");
            }
        }

        [HttpGet("CursosInscritos/{id_estudiante}")]
        public async Task<ActionResult<IEnumerable<CursoInscritoEstudiante>>> GetCursosInscritos(int id_estudiante)
        {
            var cursosInscritos = await (from estudiante in _context.Estudiante
                                         join grupo in _context.Grupo on estudiante.id_grupo equals grupo.id_grupo
                                         join periodo in _context.Periodo on grupo.id_periodo equals periodo.id_periodo
                                         join curso in _context.Curso on periodo.id_curso equals curso.id_curso
                                         join listaPeriodo in _context.ListaPeriodo on periodo.id_ListaPeriodo equals listaPeriodo.id_lista_periodo
                                         where estudiante.id_estudiante == id_estudiante
                                         select new CursoInscritoEstudiante
                                         {
                                             id_curso = curso.id_curso,
                                             nombre_curso = curso.nombre_curso,
                                             nombre_periodo = listaPeriodo.nombre_periodo,
                                             nombre_grupo = grupo.nombre_grupo,
                                             id_periodo = periodo.id_periodo
                                         }).ToListAsync();

            return Ok(cursosInscritos);
        }

        [HttpGet("sin-grupo/{id_curso}/{id_periodo}")]
        public async Task<ActionResult<IEnumerable<EstudianteConDetalles>>> GetEstudiantesSinGrupo(int id_curso, int id_periodo)
        {
            var estudiantesSinGrupo = await _context.Matricula
                .Where(m => m.id_curso == id_curso && m.id_periodo == id_periodo)
                .Join(_context.Estudiante, m => m.id_estudiante, e => e.id_estudiante, (m, e) => e)
                .Where(e => e.id_grupo == null)
                .Join(_context.Usuario, e => e.id_usuario, u => u.id_usuario, (e, u) => new { e, u })
                .Select(eu => new EstudianteConDetalles
                {
                    id_estudiante = eu.e.id_estudiante,
                    nombre_usuario = eu.u.nombre_usuario,
                    horariosPreferentes = _context.HorarioPreferenteEstudiante
                        .Where(h => h.id_estudiante == eu.e.id_estudiante)
                        .Select(h => new HorarioPreferenteEstudiante
                        {
                            dia_semana = h.dia_semana,
                            hora_inicio = h.hora_inicio,
                            hora_fin = h.hora_fin
                        }).ToList()
                }).ToListAsync();

            return Ok(estudiantesSinGrupo);
        }

        [HttpPost("HorariosPreferentes/{cursoId}/{id_estudiante}")]
        public async Task<ActionResult> GuardarHorariosPreferentes(int cursoId, int id_estudiante, List<HorarioPreferenteEstudiante> horarios)
        {
            // Obtener el estudiante usando el id_estudiante proporcionado
            var estudiante = await _context.Estudiante.FirstOrDefaultAsync(e => e.id_estudiante == id_estudiante);

            if (estudiante == null)
            {
                return NotFound("Estudiante no encontrado.");
            }

            // Verificar si el estudiante ya está inscrito en el curso
            var matricula = await _context.Matricula.FirstOrDefaultAsync(m => m.id_estudiante == estudiante.id_estudiante && m.id_curso == cursoId);

            if (matricula == null)
            {
                // Inscribir al estudiante en el curso si no está inscrito
                matricula = new Matricula
                {
                    id_estudiante = estudiante.id_estudiante,
                    id_curso = cursoId,
                    id_periodo = await ObtenerPeriodoActivo(cursoId)
                };

                _context.Matricula.Add(matricula);
                await _context.SaveChangesAsync();
            }

            // Guardar los horarios preferentes del estudiante
            foreach (var horario in horarios)
            {
                horario.id_estudiante = estudiante.id_estudiante;
                _context.HorarioPreferenteEstudiante.Add(horario);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<int> ObtenerPeriodoActivo(int cursoId)
        {
            var periodoActivo = await _context.Periodo.FirstOrDefaultAsync(p => p.id_curso == cursoId && p.activo == true);
            return periodoActivo?.id_periodo ?? 0;
        }

        [HttpGet("CursosInscritos")]
        public async Task<ActionResult<List<CursoConPeriodoActivo>>> GetCursosInscritos()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(userIdString, out int userId))
            {
                return BadRequest("Invalid user ID");
            }

            var estudiante = await _context.Estudiante.SingleOrDefaultAsync(e => e.id_usuario == userId);
            if (estudiante == null)
            {
                return NotFound("Estudiante no encontrado");
            }

            var cursosInscritos = await _context.Matricula
                .Where(m => m.id_estudiante == estudiante.id_estudiante)
                .Join(_context.Periodo, m => m.id_periodo, p => p.id_periodo, (m, p) => new { m, p })
                .Join(_context.Curso, mp => mp.p.id_curso, c => c.id_curso, (mp, c) => new { mp, c })
                .Join(_context.ListaPeriodo, mpc => mpc.mp.p.id_ListaPeriodo, lp => lp.id_lista_periodo, (mpc, lp) => new CursoConPeriodoActivo
                {
                    id_curso = mpc.c.id_curso,
                    nombre_curso = mpc.c.nombre_curso,
                    id_periodo = mpc.mp.p.id_periodo,
                    nombre_periodo = lp.nombre_periodo
                })
                .ToListAsync();

            return Ok(cursosInscritos);
        }

        [HttpGet("{id_grupo}/estudiantes")]
        public async Task<ActionResult<List<EstudianteConDetalles>>> GetEstudiantesPorGrupo(int id_grupo)
        {
            var estudiantes = await (from e in _context.Estudiante
                                     join u in _context.Usuario on e.id_usuario equals u.id_usuario
                                     where e.id_grupo == id_grupo
                                     select new EstudianteConDetalles
                                     {
                                         id_estudiante = e.id_estudiante,
                                         nombre_usuario = u.nombre_usuario,
                                         apellidos_usuario = u.apellidos_usuario,
                                         // Otros campos necesarios
                                     }).ToListAsync();

            return Ok(estudiantes);
        }



        private bool EstudianteExists(int id_estudiante)
        {
            return _context.Estudiante.Any(e => e.id_estudiante == id_estudiante);
        }
    }

    
}
