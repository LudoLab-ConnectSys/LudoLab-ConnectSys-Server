using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncuestaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EncuestaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("verificar-respuestas/{EncuestaId}/{idEstudiante}")]
        public async Task<ActionResult<bool>> VerificarRespuestas(int EncuestaId, int idEstudiante)
        {
            var existeRespuesta = await _context.Respuesta
                .AnyAsync(r => r.id_estudiante == idEstudiante && _context.Pregunta.Any(p => p.id_pregunta == r.id_pregunta && p.id_encuesta == EncuestaId));

            return Ok(existeRespuesta);
        }

        [HttpGet("preguntas/{EncuestaId}")]
        public async Task<ActionResult<List<Pregunta>>> GetPreguntas(int EncuestaId)
        {
            var preguntas = await _context.Pregunta
                .Where(p => p.id_encuesta == EncuestaId)
                .ToListAsync();

            return Ok(preguntas);
        }


        [HttpGet("preguntas")]
        public async Task<ActionResult<List<Pregunta>>> GetPreguntas()
        {
            return await _context.Pregunta.Where(p => p.id_encuesta == 1).ToListAsync();
        }

        [HttpPost("respuestas")]
        public async Task<IActionResult> PostRespuestas(List<RespuestaDto> respuestas)
        {
            if (respuestas == null || !respuestas.Any())
            {
                return BadRequest("No se han proporcionado respuestas.");
            }

            var respuestasEntities = respuestas.Select(r => new Respuesta
            {
                id_estudiante = r.id_estudiante,
                id_pregunta = r.id_pregunta,
                respuesta = r.texto_respuesta
            }).ToList();

            _context.Respuesta.AddRange(respuestasEntities);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // GET: api/Encuesta
        [HttpGet("GetEncuesta")]
        public async Task<ActionResult<IEnumerable<Encuesta>>> GetEncuestas()
        {
            return await _context.Encuesta.ToListAsync();
        }

        // GET: api/Encuesta/SumatoriaRespuestasPorPeriodo
        [HttpGet("SumatoriaRespuestasPorPeriodo")]
        public async Task<ActionResult<object>> SumatoriaRespuestasPorPeriodo(int idPeriodoF, int id_encuesta)
        {
            try
            {
                // Obtener la sumatoria de respuestas convertidas a entero para los tutores del grupo del periodo y la encuesta específica
                var sumatoriaTutores = await _context.Respuesta
                    .Join(_context.Estudiante,
                        respuesta => respuesta.id_estudiante,
                        estudiante => estudiante.id_estudiante,
                        (respuesta, estudiante) => new { respuesta, estudiante })
                    .Join(_context.Grupo,
                        est => est.estudiante.id_grupo,
                        grupo => grupo.id_grupo,
                        (est, grupo) => new { est.respuesta, est.estudiante, grupo })
                    .Join(_context.Instructor,
                        grp => grp.grupo.id_instructor,
                        instructor => instructor.id_instructor,
                        (grp, instructor) => new { grp.respuesta, grp.estudiante, grp.grupo, instructor })
                    .Join(_context.Pregunta,
                        resp => resp.respuesta.id_pregunta,
                        pregunta => pregunta.id_pregunta,
                        (resp, pregunta) => new { resp.respuesta, resp.estudiante, resp.grupo, resp.instructor, pregunta })
                    .Join(_context.Usuario,
                        inst => inst.instructor.id_usuario,
                        usuario => usuario.id_usuario,
                        (inst, usuario) => new { inst.respuesta, inst.estudiante, inst.grupo, inst.instructor, inst.pregunta, usuario })
                    .Where(x => x.pregunta.id_encuesta == id_encuesta && x.grupo.id_periodo == idPeriodoF)
                    .ToListAsync(); // Usamos ToListAsync para realizar la consulta asincrónicamente

                var sumatoriaPorTutores = sumatoriaTutores
                    .GroupBy(x => new { x.instructor.id_usuario, x.usuario.nombre_usuario, x.usuario.apellidos_usuario })
                    .Select(g => new
                    {
                        NombreCompletoTutor = $"{g.Key.nombre_usuario} {g.Key.apellidos_usuario}",
                        Sumatoria = g.Sum(x =>
                        {
                            int result;
                            if (int.TryParse(x.respuesta.respuesta, out result))
                            {
                                return result;
                            }
                            return 0;
                        })
                    })
                    .ToList();

                // Obtener la sumatoria total para todo el periodo y la encuesta específica
                var sumatoriaTotal = await _context.Respuesta
                    .Join(_context.Pregunta,
                        respuesta => respuesta.id_pregunta,
                        pregunta => pregunta.id_pregunta,
                        (respuesta, pregunta) => new { respuesta, pregunta })
                    .Where(x => x.pregunta.id_encuesta == id_encuesta)
                    .ToListAsync(); // Usamos ToListAsync para realizar la consulta asincrónicamente

                var sumatoriaTotalPeriodo = sumatoriaTotal
                    .Sum(x =>
                    {
                        int result;
                        if (int.TryParse(x.respuesta.respuesta, out result))
                        {
                            return result;
                        }
                        return 0;
                    });

                return Ok(new { SumatoriaTutores = sumatoriaPorTutores, SumatoriaTotalPeriodo = sumatoriaTotalPeriodo });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al calcular la sumatoria: {ex.Message}");
            }
        }





    }
    public class RespuestaDto
    {
        public int id_estudiante { get; set; }
        public int id_pregunta { get; set; }
        public string texto_respuesta { get; set; }
    }
    public class EstudianteNavigation
    {
        public int id_estudiante { get; set; }
        public int? id_grupo { get; set; }
        public GrupoNavigation GrupoNavigation { get; set; }
    }

    public class GrupoNavigation
    {
        public int id_grupo { get; set; }
        public int id_periodo { get; set; }
        public Instructor InstructorNavigation { get; set; }
    }
}