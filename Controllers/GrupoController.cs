using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LudoLab_ConnectSys_Server.Services;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly GrupoService _grupoService;

        public GrupoController(ApplicationDbContext context, GrupoService grupoService)
        {
            _context = context;
            _grupoService = grupoService;
        }

        // Obtener todos los grupos
        [HttpGet]
        public async Task<ActionResult<List<Grupo>>> GetGrupos()
        {
            return Ok(await _context.Grupo.ToListAsync());
        }

        // Obtener grupo por ID
        [HttpGet("{id_grupo}")]
        public async Task<ActionResult<Grupo>> GetGrupo(int id_grupo)
        {
            var grupo = await _context.Grupo.FindAsync(id_grupo);
            if (grupo == null)
            {
                return NotFound();
            }
            return Ok(grupo);
        }

        // Crear nuevo grupo
        [HttpPost]
        public async Task<ActionResult<Grupo>> CreateGrupo(Grupo grupo)
        {
            _context.Grupo.Add(grupo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGrupo), new { id_grupo = grupo.id_grupo }, grupo);
        }

        // Actualizar grupo existente
        /*[HttpPut("{id_grupo}")]
        public async Task<IActionResult> UpdateGrupo(int id_grupo, Grupo grupo)
        {
            if (id_grupo != grupo.id_grupo)
            {
                return BadRequest();
            }

            _context.Entry(grupo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GrupoExists(id_grupo))
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


        //ACTUALIZAR GRUPO
        [HttpPut("{id_grupo}")]
        public async Task<IActionResult> UpdateGrupo(int id_grupo, ActualizarGrupo grupo)
        {
            if (id_grupo != grupo.id_grupo)
            {
                return BadRequest();
            }

            var grupoExistente = await _context.Grupo.FindAsync(id_grupo);
            if (grupoExistente == null)
            {
                return NotFound();
            }

            grupoExistente.nombre_grupo = grupo.nombre_grupo;
            grupoExistente.id_instructor = grupo.id_instructor;

            // Actualizar estudiantes
            var estudiantesExistentes = await _context.Estudiante.Where(e => e.id_grupo == id_grupo).ToListAsync();
            foreach (var estudiante in estudiantesExistentes)
            {
                estudiante.id_grupo = null;
                _context.Entry(estudiante).State = EntityState.Modified;
            }

            foreach (var estudianteId in grupo.estudiantes)
            {
                var estudiante = await _context.Estudiante.FindAsync(estudianteId);
                if (estudiante != null)
                {
                    estudiante.id_grupo = id_grupo;
                    _context.Entry(estudiante).State = EntityState.Modified;
                }
            }

            // Actualizar horarios
            var horariosExistentes = await _context.Horario.Where(h => h.id_grupo == id_grupo).ToListAsync();
            _context.Horario.RemoveRange(horariosExistentes);
            await _context.SaveChangesAsync();

            foreach (var horario in grupo.horarios)
            {
                var horarioGrupo = new Horario
                {
                    id_grupo = id_grupo,
                    dia_semana = horario.dia_semana,
                    hora_inicio = horario.hora_inicio,
                    hora_fin = horario.hora_fin
                };

                _context.Horario.Add(horarioGrupo);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id_grupo}")]
        public async Task<IActionResult> DeleteGrupo(int id_grupo)
        {
            try
            {
                // Llamar al procedimiento almacenado
                await _context.Database.ExecuteSqlRawAsync("EXEC DeleteGrupo @p0", id_grupo);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log or handle the error as needed
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }





        // Obtener grupos por curso y periodo
        [HttpGet("Curso/{id_curso}/Periodo/{id_periodo}")]
        public async Task<ActionResult<List<GrupoConInstructor>>> GetGruposByCursoYPeriodo(int id_curso, int id_periodo)
        {
            var grupos = await (from g in _context.Grupo
                                join p in _context.Periodo on g.id_periodo equals p.id_periodo
                                join i in _context.Instructor on g.id_instructor equals i.id_instructor
                                join u in _context.Usuario on i.id_usuario equals u.id_usuario
                                where p.id_curso == id_curso && p.id_periodo == id_periodo
                                select new GrupoConInstructor
                                {
                                    id_grupo = g.id_grupo,
                                    id_periodo = g.id_periodo,
                                    id_instructor = g.id_instructor,
                                    nombre_instructor = u.nombre_usuario
                                }).ToListAsync();

            return Ok(grupos);
        }

        // Nuevo método para obtener grupos solo por periodo
        [HttpGet("Periodo/{id_periodo}/Grupos")]
        public async Task<ActionResult<List<GrupoConInstructor>>> GetGruposPorPeriodo(int id_periodo)
        {
            var grupos = await (from g in _context.Grupo
                                join i in _context.Instructor on g.id_instructor equals i.id_instructor
                                join u in _context.Usuario on i.id_usuario equals u.id_usuario
                                where g.id_periodo == id_periodo
                                select new GrupoConInstructor
                                {
                                    id_grupo = g.id_grupo,
                                    id_periodo = g.id_periodo,
                                    id_instructor = g.id_instructor,
                                    nombre_instructor = u.nombre_usuario + " " + u.apellidos_usuario
                                }).ToListAsync();

            return Ok(grupos);
        }
        [HttpGet]
        [Route("GetEstudiantesPorPeriodo/{id_periodo}")]
        public async Task<ActionResult<List<UsuarioPeriodo>>> GetUsuariosByPeriodo(int id_periodo)
        {
            var result = await _context.Grupo
                .Where(g => g.id_periodo == id_periodo)
                .Join(_context.Estudiante, g => g.id_grupo, e => e.id_grupo, (g, e) => new { g, e })
                .Join(_context.Usuario, ge => ge.e.id_usuario, u => u.id_usuario, (ge, u) => new { ge, u })
                .Join(_context.Certificado, geu => geu.ge.g.id_periodo, c => c.id_periodo, (geu, c) => new UsuarioPeriodo // Join con Certificado
                {
                    IdUsuario = geu.u.id_usuario,
                    CedulaUsuario = geu.u.cedula_usuario,
                    NombreUsuario = geu.u.nombre_usuario,
                    ApellidosUsuario = geu.u.apellidos_usuario,
                    EdadUsuario = geu.u.edad_usuario,
                    CorreoUsuario = geu.u.correo_usuario,
                    CelularUsuario = geu.u.celular_usuario,
                    TelefonoUsuario = geu.u.telefono_usuario,
                    horas_asignadas_estudiante = geu.ge.e.horas_asignadas_estudiante,
                    IdGrupo = geu.ge.g.id_grupo,
                    IdPeriodo = geu.ge.g.id_periodo,
                    IdInstructor = geu.ge.g.id_instructor,
                    NombreCertificado = c.nombre_certificado // Obtener el nombre del certificado
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("GetInstructoresPorPeriodo/{id_periodo}")]
        public async Task<ActionResult<List<InstructorPeriodo>>> GetInstructoresPorPeriodo(int id_periodo)
        {
            var result = await (from i in _context.Instructor
                                join u in _context.Usuario on i.id_usuario equals u.id_usuario
                                join h in _context.Horas_instructor on i.id_instructor equals h.id_instructor
                                join p in _context.Periodo on h.id_periodo equals p.id_periodo
                                join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo
                                join c in _context.Curso on p.id_curso equals c.id_curso
                                join cert in _context.Certificado on p.id_periodo equals cert.id_periodo
                                where h.id_periodo == id_periodo
                                select new InstructorPeriodo
                                {
                                    IdUsuario = u.id_usuario,
                                    CedulaUsuario = u.cedula_usuario,
                                    NombreUsuario = u.nombre_usuario,
                                    ApellidosUsuario = u.apellidos_usuario,
                                    EdadUsuario = u.edad_usuario,
                                    CorreoUsuario = u.correo_usuario,
                                    CelularUsuario = u.celular_usuario,
                                    TelefonoUsuario = u.telefono_usuario,
                                    HorasGanadasInstructor = h.horas_ganadas_instructor,
                                    NombreCertificado = cert.nombre_certificado,
                                    NombreCurso = c.nombre_curso,
                                    NombrePeriodo = lp.nombre_periodo
                                }).ToListAsync();

            return Ok(result);
        }

        // Crear Grupos
        [HttpPost("CrearGrupos")]
        public async Task<ActionResult<List<GrupoConDetalles>>> CrearGrupos(CrearGruposParametros parametros)
        {
            var gruposCreados = await _grupoService.CrearGruposAsync(parametros);
            return Ok(gruposCreados);
        }

        // Previsualizar Grupos
        [HttpPost("PrevisualizarGrupos")]
        public async Task<ActionResult<List<GrupoConDetalles>>> PrevisualizarGrupos(CrearGruposParametros parametros)
        {
            var gruposPrevisualizados = await _grupoService.CrearGruposAsync(parametros);
            return Ok(gruposPrevisualizados);
        }

        //GUARDAR GRUPOS PREVISUALIZADOS
        [HttpPost("GuardarGrupos")]
        public async Task<IActionResult> GuardarGrupos(List<GrupoConDetalles> grupos)
        {
            foreach (var grupo in grupos)
            {
                var nuevoGrupo = new Grupo
                {
                    id_periodo = grupo.id_periodo,
                    id_instructor = grupo.id_instructor,
                    nombre_grupo = grupo.nombre_grupo
                };

                _context.Grupo.Add(nuevoGrupo);
                await _context.SaveChangesAsync();

                // Asignar estudiantes a grupos
                if (grupo.estudiantes != null)
                {
                    foreach (var estudianteNombre in grupo.estudiantes)
                    {
                        var estudiante = await _context.Estudiante
                            .Join(_context.Usuario, e => e.id_usuario, u => u.id_usuario, (e, u) => new { e, u })
                            .Where(eu => eu.u.nombre_usuario == estudianteNombre)
                            .Select(eu => eu.e)
                            .FirstOrDefaultAsync();

                        if (estudiante != null)
                        {
                            estudiante.id_grupo = nuevoGrupo.id_grupo;
                            _context.Entry(estudiante).State = EntityState.Modified;
                        }
                    }
                }

                // Añadir horario del grupo
                if (grupo.horarios != null)
                {
                    foreach (var horario in grupo.horarios)
                    {
                        var horarioGrupo = new Horario
                        {
                            id_grupo = nuevoGrupo.id_grupo,
                            dia_semana = horario.dia_semana,
                            hora_inicio = horario.hora_inicio,
                            hora_fin = horario.hora_fin
                        };

                        _context.Horario.Add(horarioGrupo);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        //CREAR GRUPO MANUAL
        [HttpPost("CrearGrupoManual")]
        public async Task<IActionResult> CrearGrupoManual(CrearGrupoManualParametros parametros)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var nombreGrupo = await GenerarNombreGrupo(parametros.PeriodoId);
            var grupo = new Grupo
            {
                id_periodo = parametros.PeriodoId,
                id_instructor = parametros.InstructorId,
                nombre_grupo = nombreGrupo
            };

            _context.Grupo.Add(grupo);
            await _context.SaveChangesAsync();

            foreach (var estudianteId in parametros.EstudiantesSeleccionados)
            {
                var estudiante = await _context.Estudiante.FindAsync(estudianteId);
                if (estudiante != null)
                {
                    estudiante.id_grupo = grupo.id_grupo;
                    _context.Entry(estudiante).State = EntityState.Modified;
                }
            }

            foreach (var horario in parametros.Horarios)
            {
                var horarioGrupo = new Horario
                {
                    id_grupo = grupo.id_grupo,
                    dia_semana = horario.dia_semana,
                    hora_inicio = horario.hora_inicio,
                    hora_fin = horario.hora_fin
                };

                _context.Horario.Add(horarioGrupo);
            }

            await _context.SaveChangesAsync();

            return Ok(grupo);
        }

        //Detalles del Grupo por ID
        [HttpGet("{id_grupo}/detalles")]
        public async Task<ActionResult<GrupoConDetalles>> GetGrupoDetalles(int id_grupo)
        {
            var grupo = await (from g in _context.Grupo
                               join i in _context.Instructor on g.id_instructor equals i.id_instructor
                               join u in _context.Usuario on i.id_usuario equals u.id_usuario
                               where g.id_grupo == id_grupo
                               select new GrupoConDetalles
                               {
                                   id_grupo = g.id_grupo,
                                   id_periodo = g.id_periodo,
                                   id_instructor = g.id_instructor,
                                   nombre_instructor = u.nombre_usuario + " " + u.apellidos_usuario,
                                   nombre_grupo = g.nombre_grupo ?? string.Empty,
                                   estudiantes = (from e in _context.Estudiante
                                                  join ue in _context.Usuario on e.id_usuario equals ue.id_usuario
                                                  where e.id_grupo == g.id_grupo
                                                  select ue.nombre_usuario + " " + ue.apellidos_usuario).ToList()
                               }).FirstOrDefaultAsync();

            if (grupo == null)
            {
                return NotFound();
            }

            return Ok(grupo);
        }

        //Estudiantes del Grupo por ID  del grupo
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
                                         edad_usuario = u.edad_usuario,
                                         correo_usuario = u.correo_usuario,
                                         cedula_usuario = u.cedula_usuario
                                         // Otros campos necesarios
                                     }).ToListAsync();

            return Ok(estudiantes);
        }

        //Crear nombre del grupo automaticamente
        private async Task<string> GenerarNombreGrupo(int periodoId)
        {
            var ultimoGrupo = await _context.Grupo
                .Where(g => g.id_periodo == periodoId)
                .OrderByDescending(g => g.id_grupo)
                .Select(g => g.nombre_grupo)
                .FirstOrDefaultAsync();

            int contadorGrupo = 1;
            if (ultimoGrupo != null)
            {
                var partes = ultimoGrupo.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNumero))
                {
                    contadorGrupo = ultimoNumero + 1;
                }
            }

            return $"GR-{contadorGrupo}";
        }


        private bool GrupoExists(int id_grupo)
        {
            return _context.Grupo.Any(e => e.id_grupo == id_grupo);
        }
    }
}