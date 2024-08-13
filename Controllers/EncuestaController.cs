using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
                // Obtener todos los tutores del periodo y agruparlos por tutor (incluyendo todos los grupos)
                var tutores = await _context.Instructor
                    .Join(_context.Grupo,
                        instructor => instructor.id_instructor,
                        grupo => grupo.id_instructor,
                        (instructor, grupo) => new { instructor, grupo })
                    .Join(_context.Usuario,
                        inst => inst.instructor.id_usuario,
                        usuario => usuario.id_usuario,
                        (inst, usuario) => new { inst.instructor, inst.grupo, usuario })
                    .Where(x => x.grupo.id_periodo == idPeriodoF)
                    .GroupBy(x => new { x.instructor.id_instructor, x.usuario.nombre_usuario, x.usuario.apellidos_usuario })
                    .Select(g => new
                    {
                        g.Key.id_instructor,
                        NombreCompletoTutor = $"{g.Key.nombre_usuario} {g.Key.apellidos_usuario}",
                        Grupos = g.Select(x => x.grupo.id_grupo).ToList()
                    })
                    .ToListAsync();

                // Obtener la sumatoria de respuestas para todos los grupos del periodo y encuesta específica
                var respuestas = await _context.Respuesta
                    .Join(_context.Pregunta,
                        respuesta => respuesta.id_pregunta,
                        pregunta => pregunta.id_pregunta,
                        (respuesta, pregunta) => new { respuesta, pregunta })
                    .Join(_context.Estudiante,
                        resp => resp.respuesta.id_estudiante,
                        estudiante => estudiante.id_estudiante,
                        (resp, estudiante) => new { resp.respuesta, resp.pregunta, estudiante })
                    .Join(_context.Grupo,
                        est => est.estudiante.id_grupo,
                        grupo => grupo.id_grupo,
                        (est, grupo) => new { est.respuesta, est.pregunta, est.estudiante, grupo })
                    .Where(x => x.pregunta.id_encuesta == id_encuesta && x.grupo.id_periodo == idPeriodoF)
                    .ToListAsync();

                // Agrupar respuestas por tutor y calcular la sumatoria y las notas por pregunta
                var sumatoriaPorTutores = tutores
                    .Select(tutor => new
                    {
                        tutor.NombreCompletoTutor,
                        Sumatoria = respuestas
                            .Where(x => tutor.Grupos.Contains(x.grupo.id_grupo))
                            .Sum(x =>
                            {
                                int result;
                                if (int.TryParse(x.respuesta.respuesta, out result))
                                {
                                    return result;
                                }
                                return 0;
                            }),
                        NotasPorPregunta = respuestas
                            .Where(x => tutor.Grupos.Contains(x.grupo.id_grupo))
                            .GroupBy(x => new { x.pregunta.id_pregunta, x.pregunta.texto_pregunta })
                            .Select(g => new
                            {
                                g.Key.id_pregunta,
                                g.Key.texto_pregunta,
                                Nota = g.Sum(x =>
                                {
                                    int result;
                                    if (int.TryParse(x.respuesta.respuesta, out result))
                                    {
                                        return result;
                                    }
                                    return 0;
                                })
                            })
                            .ToList()
                    })
                    .ToList();

                // Obtener la sumatoria total para todo el periodo y la encuesta específica
                var sumatoriaTotal = respuestas
                    .Sum(x =>
                    {
                        int result;
                        if (int.TryParse(x.respuesta.respuesta, out result))
                        {
                            return result;
                        }
                        return 0;
                    });

                return Ok(new { SumatoriaTutores = sumatoriaPorTutores, SumatoriaTotalPeriodo = sumatoriaTotal });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al calcular la sumatoria: {ex.Message}");
            }
        }




        [HttpGet("getnombreencuesta/{id}")]
        public async Task<ActionResult<string>> GetEncuestaTitulo(int id)
        {
            var encuesta = await _context.Encuesta
                .Where(e => e.id_encuesta == id)
                .Select(e => e.titulo)
                .FirstOrDefaultAsync();

            if (encuesta == null)
            {
                return NotFound("Encuesta no encontrada");
            }

            return Ok(encuesta);
        }
        /*-------------------- CRUD ENCUESTAS --------------------*/

        // GET: api/Encuesta
        [HttpGet("GetTodaEncuesta")]
        public async Task<ActionResult<IEnumerable<Encuesta>>> GetTodaEncuestas()
        {
            return await _context.Encuesta.ToListAsync();
        }

        [HttpGet("preguntasporID/{id}")]
        public async Task<ActionResult<List<Pregunta>>> GetPreguntasporID(int id)
        {
            var preguntas = await _context.Pregunta
                .Where(p => p.id_encuesta == id)
                .ToListAsync();

            return Ok(preguntas);
        }

        // PUT: api/Encuesta/ActualizarPreguntas
        [HttpPut("ActualizarPreguntas/{id_encuesta}")]
        public async Task<IActionResult> ActualizarPreguntas(int id_encuesta, [FromBody] List<Pregunta> preguntas)
        {
            if (preguntas == null || !preguntas.Any())
            {
                return BadRequest("La lista de preguntas no puede estar vacía.");
            }

            var encuestaExistente = await _context.Encuesta.FindAsync(id_encuesta);
            if (encuestaExistente == null)
            {
                return NotFound("Encuesta no encontrada.");
            }

            foreach (var pregunta in preguntas)
            {
                var preguntaExistente = await _context.Pregunta
                    .FirstOrDefaultAsync(p => p.id_pregunta == pregunta.id_pregunta && p.id_encuesta == id_encuesta);

                if (preguntaExistente != null)
                {
                    preguntaExistente.texto_pregunta = pregunta.texto_pregunta;
                    _context.Pregunta.Update(preguntaExistente);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error al actualizar las preguntas.");
            }

            return NoContent();
        }

        /*------------------------- API para crear encuesta -------------------------*/
        // POST: api/Encuesta/CrearEncuesta
        [HttpPost("CrearEncuesta")]
        public async Task<IActionResult> CrearEncuesta([FromBody] EncuestaConPreguntasModel model)
        {
            if (model == null || model.Preguntas == null || !model.Preguntas.Any())
            {
                return BadRequest("Datos inválidos para crear la encuesta.");
            }

            var nuevaEncuesta = new Encuesta
            {
                titulo = model.Titulo,
                fecha_creacion = DateTime.Now
            };

            _context.Encuesta.Add(nuevaEncuesta);
            await _context.SaveChangesAsync();

            foreach (var preguntaTexto in model.Preguntas)
            {
                var nuevaPregunta = new Pregunta
                {
                    id_encuesta = nuevaEncuesta.id_encuesta,
                    texto_pregunta = preguntaTexto
                };
                _context.Pregunta.Add(nuevaPregunta);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Error al crear la encuesta.");
            }

            return CreatedAtAction(nameof(CrearEncuesta), new { id = nuevaEncuesta.id_encuesta }, nuevaEncuesta);
        }

        [HttpDelete("EliminarPreguntas/{id}")]
        public async Task<IActionResult> EliminarPreguntas(int id)
        {
            // Buscar las preguntas asociadas a la encuesta
            var preguntas = await _context.Pregunta
                .Where(p => p.id_encuesta == id)
                .ToListAsync();

            if (preguntas.Count == 0)
            {
                return NotFound("No se encontraron preguntas para la encuesta proporcionada.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var pregunta in preguntas)
                    {
                        // Verificar si la pregunta tiene respuestas asociadas
                        var respuestas = await _context.Respuesta
                            .Where(r => r.id_pregunta == pregunta.id_pregunta)
                            .ToListAsync();

                        if (respuestas.Count > 0)
                        {
                            // Si tiene respuestas asociadas, no se puede eliminar
                            return BadRequest("No se puede eliminar la pregunta porque tiene respuestas asociadas.");
                        }

                        // Eliminar la pregunta si no tiene respuestas asociadas
                        _context.Pregunta.Remove(pregunta);
                    }

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    // Confirmar la transacción
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Deshacer la transacción en caso de error
                    await transaction.RollbackAsync();
                    // Manejo de errores
                    return StatusCode(500, "Error al eliminar las preguntas: " + ex.Message);
                }
            }

            return Ok("Preguntas eliminadas con éxito.");
        }
        [HttpDelete("EliminarEncuesta/{id}")]
        public async Task<IActionResult> EliminarEncuesta(int id)
        {
            // Buscar la encuesta con el id proporcionado
            var encuesta = await _context.Encuesta
                .FirstOrDefaultAsync(e => e.id_encuesta == id);

            if (encuesta == null)
            {
                return NotFound("Encuesta no encontrada.");
            }

            // Comenzar una transacción para asegurar la consistencia
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Eliminar la encuesta
                    _context.Encuesta.Remove(encuesta);

                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();

                    // Confirmar la transacción
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Deshacer la transacción en caso de error
                    await transaction.RollbackAsync();
                    // Manejo de errores
                    return StatusCode(500, "Error al eliminar la encuesta: " + ex.Message);
                }
            }

            return Ok("Encuesta eliminada con éxito.");
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
    public class EncuestaConPreguntasModel 
    {// Modelo para la solicitud de creación de encuesta
        public string Titulo { get; set; }
        public List<string> Preguntas { get; set; }
    }
}