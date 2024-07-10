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

        // PUT: api/Instructor/{id_instructor}
        [HttpPut("{id_instructor}")]
        public async Task<IActionResult> PutInstructor(int id_instructor, Instructor instructor)
        {
            if (id_instructor != instructor.id_instructor)
            {
                return BadRequest();
            }

            _context.Entry(instructor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InstructorExists(id_instructor))
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

        // DELETE: api/Instructor/{id_instructor}
        [HttpDelete("{id_instructor}")]
        public async Task<IActionResult> DeleteInstructor(int id_instructor)
        {
            var instructor = await _context.Instructor.FindAsync(id_instructor);
            if (instructor == null)
            {
                return NotFound();
            }

            _context.Instructor.Remove(instructor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InstructorExists(int id_instructor)
        {
            return _context.Instructor.Any(e => e.id_instructor == id_instructor);
        }

        [HttpGet("instructoresPracticas")]
        public async Task<ActionResult<IEnumerable<InstructorDto>>> GetInstructoresPracticas(int pageIndex, int pageSize)
        {
            var instructoresQuery = (
                from instructor in _context.Instructor
                join usuario in _context.Usuario on instructor.id_usuario equals usuario.id_usuario
                select new InstructorDto
                {
                    IdInstructor = instructor.id_instructor,
                    CedulaUsuario = usuario.cedula_usuario ?? string.Empty,
                    NombreUsuario = usuario.nombre_usuario ?? string.Empty,
                    ApellidosUsuario = usuario.apellidos_usuario ?? string.Empty,
                    EdadUsuario = usuario.edad_usuario,
                    CorreoUsuario = usuario.correo_usuario ?? string.Empty,
                    CelularUsuario = usuario.celular_usuario ?? string.Empty,
                    TelefonoUsuario = usuario.telefono_usuario ?? string.Empty,
                    Grupos = (
                        from grupo in _context.Grupo
                        where grupo.id_instructor == instructor.id_instructor
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
                    ).ToList(),
                    TotalHorasGanadas = _context.Horas_instructor
                                            .Where(h => h.id_instructor == instructor.id_instructor)
                                            .Sum(h => (int?)h.horas_ganadas_instructor) ?? 0
                }
            );

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
