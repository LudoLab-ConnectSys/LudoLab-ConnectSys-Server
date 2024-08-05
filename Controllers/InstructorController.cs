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
    public class InstructorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Instructor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Instructor>>> GetInstructores()
        {
            return await _context.Instructor.ToListAsync();
        }

        // GET: api/Instructor/{id_instructor}
        [HttpGet("{id_instructor}")]
        public async Task<ActionResult<Instructor>> GetInstructor(int id_instructor)
        {
            var instructor = await _context.Instructor.FindAsync(id_instructor);

            if (instructor == null)
            {
                return NotFound();
            }

            return instructor;
        }

        // POST: api/Instructor
        [HttpPost]
        public async Task<ActionResult<Instructor>> PostInstructor(Instructor instructor)
        {
            _context.Instructor.Add(instructor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetInstructor", new { id_instructor = instructor.id_instructor }, instructor);
        }

        [HttpPut("{id_instructor}")]
        public async Task<IActionResult> UpdateInstructor(int id_instructor, [FromBody] InstructorUpdateModel model)
        {
            if (id_instructor != model.id_instructor)
            {
                return BadRequest();
            }

            var parameterId = new SqlParameter("@id_instructor", id_instructor);
            var parameterNombre = new SqlParameter("@nombre_usuario", model.nombre_usuario);
            var parameterApellidos = new SqlParameter("@apellidos_usuario", model.apellidos_usuario);
            var parameterCedula = new SqlParameter("@cedula_usuario", model.cedula_usuario);
            var parameterEdad = new SqlParameter("@edad_usuario", model.edad_usuario);
            var parameterCorreo = new SqlParameter("@correo_usuario", model.correo_usuario);
            var parameterCelular = new SqlParameter("@celular_usuario", model.celular_usuario);
            var parameterTelefono = new SqlParameter("@telefono_usuario", model.telefono_usuario);

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_ActualizarInstructor @id_instructor, @nombre_usuario, @apellidos_usuario, @cedula_usuario, @edad_usuario, @correo_usuario, @celular_usuario, @telefono_usuario",
                    parameterId, parameterNombre, parameterApellidos, parameterCedula, parameterEdad, parameterCorreo, parameterCelular, parameterTelefono);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return NoContent();
        }

        [HttpDelete("{id_instructor}")]
        public async Task<IActionResult> DeleteInstructor(int id_instructor)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC sp_EliminarInstructor @id_instructor", new SqlParameter("@id_instructor", id_instructor));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

            return NoContent();
        }


        [HttpGet("Detalles")]
        public async Task<ActionResult<IEnumerable<InstructorConDetalles>>> GetInstructoresConDetalles()
        {
            var instructores = await _context.Instructor
                .Join(_context.Usuario, i => i.id_usuario, u => u.id_usuario, (i, u) => new { i, u })
                .GroupBy(x => new { x.i.id_instructor, x.u.nombre_usuario, x.u.apellidos_usuario, x.u.correo_usuario })
                .Select(group => new InstructorConDetalles
                {
                    id_instructor = group.Key.id_instructor,
                    nombre_usuario = group.Key.nombre_usuario,
                    apellidos_usuario = group.Key.apellidos_usuario,
                    correo_usuario = group.Key.correo_usuario,
                    cursos = _context.RegistroInstructor
                                .Where(ri => ri.id_instructor == group.Key.id_instructor)
                                .Join(_context.Curso, ri => ri.id_curso, c => c.id_curso, (ri, c) => c.nombre_curso)
                                .ToList(),
                    periodos = _context.RegistroInstructor
                                .Where(ri => ri.id_instructor == group.Key.id_instructor)
                                .Join(_context.Periodo, ri => ri.id_periodo, p => p.id_periodo, (ri, p) => _context.ListaPeriodo
                                        .Where(lp => lp.id_lista_periodo == p.id_ListaPeriodo)
                                        .Select(lp => lp.nombre_periodo)
                                        .FirstOrDefault())
                                .ToList(),
                    id_cursos = _context.RegistroInstructor
                                .Where(ri => ri.id_instructor == group.Key.id_instructor)
                                .Select(ri => ri.id_curso)
                                .ToList(),
                    id_periodos = _context.RegistroInstructor
                                .Where(ri => ri.id_instructor == group.Key.id_instructor)
                                .Select(ri => ri.id_periodo)
                                .ToList()
                })
                .ToListAsync();

            return Ok(instructores);
        }


        [HttpGet("sin-grupo/{id_curso}/{id_periodo}")]
        public async Task<ActionResult<IEnumerable<InstructorConDetalles>>> GetInstructoresSinGrupo(int id_curso, int id_periodo)
        {
            var instructoresSinGrupo = await _context.RegistroInstructor
                .Where(ri => ri.id_curso == id_curso && ri.id_periodo == id_periodo)
                .Join(_context.Instructor, ri => ri.id_instructor, i => i.id_instructor, (ri, i) => i)
                .Where(i => !_context.Grupo.Any(g => g.id_instructor == i.id_instructor && g.id_periodo == id_periodo))
                .Join(_context.Usuario, i => i.id_usuario, u => u.id_usuario, (i, u) => new { i, u })
                .Select(iu => new InstructorConDetalles
                {
                    id_instructor = iu.i.id_instructor,
                    nombre_usuario = iu.u.nombre_usuario,
                    horariosPreferentes = _context.HorarioPreferenteInstructor
                        .Where(h => h.id_instructor == iu.i.id_instructor)
                        .Select(h => new HorarioPreferenteInstructor
                        {
                            dia_semana = h.dia_semana,
                            hora_inicio = h.hora_inicio,
                            hora_fin = h.hora_fin
                        }).ToList()
                }).ToListAsync();

            return Ok(instructoresSinGrupo);
        }

        [HttpGet("todos")]
        public async Task<ActionResult<IEnumerable<InstructorConDetalles>>> GetTodosInstructores()
        {
            var instructores = await (from instructor in _context.Instructor
                                      join usuario in _context.Usuario on instructor.id_usuario equals usuario.id_usuario
                                      select new InstructorConDetalles
                                      {
                                          id_instructor = instructor.id_instructor,
                                          nombre_usuario = usuario.nombre_usuario + " " + usuario.apellidos_usuario
                                      }).ToListAsync();

            return Ok(instructores);
        }

        [HttpPost("RegistrarHorarios")]
        public async Task<IActionResult> RegistrarHorarios(HorariosInstructor model)
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

            var instructor = await _context.Instructor.SingleOrDefaultAsync(i => i.id_usuario == userId);
            if (instructor == null)
            {
                return NotFound("Instructor no encontrado");
            }

            foreach (var horario in model.Horarios)
            {
                _context.HorarioPreferenteInstructor.Add(new HorarioPreferenteInstructor
                {
                    id_instructor = instructor.id_instructor,
                    dia_semana = horario.dia_semana,
                    hora_inicio = horario.hora_inicio,
                    hora_fin = horario.hora_fin
                });
            }

            // Register instructor for the course and period
            _context.RegistroInstructor.Add(new RegistroInstructor
            {
                id_instructor = instructor.id_instructor,
                id_curso = model.CursoId,
                id_periodo = model.PeriodoId
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("DetallesById/{id_instructor}")]
        public async Task<ActionResult<InstructorConDetallesCompleto>> GetInstructorDetallesById(int id_instructor)
        {
            var instructor = await _context.Instructor
                .Where(i => i.id_instructor == id_instructor)
                .Join(_context.Usuario, i => i.id_usuario, u => u.id_usuario, (i, u) => new { i, u })
                .Select(iu => new InstructorConDetallesCompleto
                {
                    id_instructor = iu.i.id_instructor,
                    nombre_usuario = iu.u.nombre_usuario,
                    apellidos_usuario = iu.u.apellidos_usuario,
                    cedula_usuario = iu.u.cedula_usuario,
                    correo_usuario = iu.u.correo_usuario,
                    celular_usuario = iu.u.celular_usuario,
                    telefono_usuario = iu.u.telefono_usuario,
                    id_cursos = new List<int>(), // Agrega el mapeo de id_cursos si es necesario
                    id_periodos = new List<int>(), // Agrega el mapeo de id_periodos si es necesario
                    cursos = new List<string>(), // Agrega el mapeo de cursos si es necesario
                    periodos = new List<string>(), // Agrega el mapeo de periodos si es necesario
                    horariosPreferentes = new List<HorarioPreferenteInstructor>() // Agrega el mapeo de horarios si es necesario
                })
                .FirstOrDefaultAsync();

            if (instructor == null)
            {
                return NotFound();
            }

            return Ok(instructor);
        }


        private bool InstructorExists(int id_instructor)
        {
            return _context.Instructor.Any(e => e.id_instructor == id_instructor);
        }

        [HttpGet("instructoresPracticas")]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructoresPracticas(int pageIndex, int pageSize, int id_periodo)
        {
            var instructoresQuery = (
                from instructor in _context.Instructor
                join usuario in _context.Usuario on instructor.id_usuario equals usuario.id_usuario
                select new
                {
                    Instructor = instructor,
                    Usuario = usuario,
                    Grupos = (
                        from grupo in _context.Grupo
                        where grupo.id_instructor == instructor.id_instructor && grupo.id_periodo == id_periodo
                        select new GrupoDto
                        {
                            IdGrupo = grupo.id_grupo,
                            IdPeriodo = grupo.id_periodo,
                            NombreGrupo = grupo.nombre_grupo ?? string.Empty,
                            Horarios = _context.Horario
                                        .Where(h => h.id_grupo == grupo.id_grupo)
                                        .Select(h => new Horario
                                        {
                                            id_horario = h.id_horario,
                                            id_grupo = h.id_grupo,
                                            dia_semana = h.dia_semana ?? string.Empty,
                                            hora_inicio = h.hora_inicio,
                                            hora_fin = h.hora_fin
                                        })
                                        .ToList(),
                        }
                    ).ToList()
                }
            )
            .Where(x => x.Grupos.Any()) // Filtrar instructores que tienen grupos en el período dado
            .Select(x => new InstructorDto
            {
                IdInstructor = x.Instructor.id_instructor,
                CedulaUsuario = x.Usuario.cedula_usuario ?? string.Empty,
                NombreUsuario = x.Usuario.nombre_usuario ?? string.Empty,
                ApellidosUsuario = x.Usuario.apellidos_usuario ?? string.Empty,
                EdadUsuario = x.Usuario.edad_usuario,
                CorreoUsuario = x.Usuario.correo_usuario ?? string.Empty,
                CelularUsuario = x.Usuario.celular_usuario ?? string.Empty,
                TelefonoUsuario = x.Usuario.telefono_usuario ?? string.Empty,
                Grupos = x.Grupos,
                TotalHorasGanadas = _context.Horas_instructor
                                        .Where(h => h.id_instructor == x.Instructor.id_instructor)
                                        .Sum(h => (int?)h.horas_ganadas_instructor) ?? 0,
                IdPeriodo = id_periodo
            });

            var totalInstructores = await instructoresQuery.CountAsync();

            var instructoresPaginados = await instructoresQuery
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new InstructoresPaginadosDto
            {
                Instructores = instructoresPaginados,
                TotalCount = totalInstructores
            };

            return Ok(response);
        }



    }

}
