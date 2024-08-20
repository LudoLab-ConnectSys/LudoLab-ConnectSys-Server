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
        public async Task<IActionResult> PostRespuestas(List<RespuestaDto> respuestasEnviar)
        {
            if (respuestasEnviar == null || !respuestasEnviar.Any())
            {
                return BadRequest("No se han proporcionado respuestas.");
            }

            try
            {
                var respuestasEntities = respuestasEnviar.Select(r => new Respuesta
                {
                    id_estudiante = r.id_estudiante,
                    id_pregunta = r.id_pregunta,
                    id_opcion = r.id_opcion
                }).ToList();

                _context.Respuesta.AddRange(respuestasEntities);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                // Log de la excepción completo para más detalles
                Console.WriteLine($"Error al guardar respuestas: {ex}");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
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
                // Obtener todas las respuestas relevantes basadas en el periodo y la encuesta
                var respuestas = await _context.Respuesta
                    .Join(_context.Pregunta,
                        respuesta => respuesta.id_pregunta,
                        pregunta => pregunta.id_pregunta,
                        (respuesta, pregunta) => new { respuesta, pregunta })
                    .Join(_context.Opcion,
                        resp => resp.respuesta.id_opcion,
                        opcion => opcion.id_opcion,
                        (resp, opcion) => new { resp.respuesta, resp.pregunta, opcion })
                    .Join(_context.Estudiante,
                        resp => resp.respuesta.id_estudiante,
                        estudiante => estudiante.id_estudiante,
                        (resp, estudiante) => new { resp.respuesta, resp.pregunta, resp.opcion, estudiante })
                    .Join(_context.Grupo,
                        est => est.estudiante.id_grupo,
                        grupo => grupo.id_grupo,
                        (est, grupo) => new { est.respuesta, est.pregunta, est.opcion, est.estudiante, grupo })
                    .Where(x => x.pregunta.id_encuesta == id_encuesta && x.grupo.id_periodo == idPeriodoF)
                    .ToListAsync();

                // Obtener el número de personas que respondieron la encuesta
                var numeroDePersonas = respuestas
                    .Select(x => x.respuesta.id_estudiante)
                    .Distinct()
                    .Count();

                // Calcular la sumatoria total de las respuestas (valores de las opciones) en todo el periodo y encuesta
                var sumatoriaTotal = respuestas
                    .Sum(x => x.opcion.valor_numero);

                // Calcular la sumatoria por pregunta y por opción
                var sumatoriaPorPregunta = respuestas
                    .GroupBy(x => new { x.pregunta.id_pregunta, x.pregunta.texto_pregunta })
                    .Select(g => new
                    {
                        g.Key.id_pregunta,
                        g.Key.texto_pregunta,
                        SumatoriaPorPregunta = g.Sum(x => x.opcion.valor_numero),
                        SumatoriaPorOpcion = g
                            .GroupBy(x => new { x.opcion.id_opcion, x.opcion.texto_opcion })
                            .Select(o => new
                            {
                                o.Key.id_opcion,
                                o.Key.texto_opcion,
                                Sumatoria = o.Sum(x => x.opcion.valor_numero),
                                NumeroDeRespuestas = o.Count()
                            })
                            .ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    NumeroDePersonas = numeroDePersonas,
                    SumatoriaTotalPeriodo = sumatoriaTotal,
                    SumatoriaPorPregunta = sumatoriaPorPregunta
                });
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
        public async Task<ActionResult<List<PreguntaConOpciones>>> GetPreguntasporID(int id)
        {
            var preguntas = await _context.Pregunta
                .Where(p => p.id_encuesta == id)
                .Select(p => new PreguntaConOpciones
                {
                    IdPregunta = p.id_pregunta,
                    TextoPregunta = p.texto_pregunta,
                    Opciones = _context.Opcion
                        .Where(o => o.id_pregunta == p.id_pregunta)
                        .Select(o => new OpcionDto
                        {
                            idOption = o.id_opcion,
                            Texto = o.texto_opcion,
                            Valor = o.valor_numero
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(preguntas);
        }

        // PUT: api/Encuesta/ActualizarPreguntas
        [HttpPut("ActualizarPreguntas/{id_encuesta}")]
        public async Task<IActionResult> ActualizarPreguntas(int id_encuesta, [FromBody] List<PreguntaConOpciones> preguntasConOpciones)
        {
            if (preguntasConOpciones == null || !preguntasConOpciones.Any())
            {
                return BadRequest("La lista de preguntas no puede estar vacía.");
            }

            var encuestaExistente = await _context.Encuesta.FindAsync(id_encuesta);
            if (encuestaExistente == null)
            {
                return NotFound("Encuesta no encontrada.");
            }

            foreach (var preguntaConOpciones in preguntasConOpciones)
            {
                var preguntaExistente = await _context.Pregunta
                    .FirstOrDefaultAsync(p => p.id_pregunta == preguntaConOpciones.IdPregunta && p.id_encuesta == id_encuesta);

                if (preguntaExistente != null)
                {
                    preguntaExistente.texto_pregunta = preguntaConOpciones.TextoPregunta;
                    _context.Pregunta.Update(preguntaExistente);

                    var opcionesExistentes = await _context.Opcion
                        .Where(o => o.id_pregunta == preguntaConOpciones.IdPregunta)
                        .ToListAsync();

                    // Eliminar opciones antiguas
                    _context.Opcion.RemoveRange(opcionesExistentes);

                    // Agregar nuevas opciones
                    foreach (var opcion in preguntaConOpciones.Opciones)
                    {
                        _context.Opcion.Add(new Opcion
                        {
                            id_pregunta = preguntaConOpciones.IdPregunta,
                            texto_opcion = opcion.Texto,
                            valor_numero = opcion.Valor
                        });
                    }
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

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Crear la encuesta
                var nuevaEncuesta = new Encuesta
                {
                    titulo = model.Titulo,
                    fecha_creacion = DateTime.Now
                };

                _context.Encuesta.Add(nuevaEncuesta);
                await _context.SaveChangesAsync();

                // Insertar preguntas
                foreach (var pregunta in model.Preguntas)
                {
                    var nuevaPregunta = new Pregunta
                    {
                        id_encuesta = nuevaEncuesta.id_encuesta,
                        texto_pregunta = pregunta.TextoPregunta
                    };

                    _context.Pregunta.Add(nuevaPregunta);
                    await _context.SaveChangesAsync();

                    // Insertar opciones para cada pregunta
                    foreach (var opcion in pregunta.Opciones)
                    {
                        var nuevaOpcion = new Opcion
                        {
                            id_pregunta = nuevaPregunta.id_pregunta,
                            texto_opcion = opcion.Texto,
                            valor_numero = opcion.Valor
                        };

                        _context.Opcion.Add(nuevaOpcion);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(CrearEncuesta), new { id = nuevaEncuesta.id_encuesta }, nuevaEncuesta);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error al crear la encuesta: {ex.Message}");
            }
        }

        [HttpDelete("EliminarOpciones/{id_encuesta}")]
        public async Task<IActionResult> EliminarOpciones(int id_encuesta)
        {
            // Buscar las opciones asociadas a la encuesta
            var opciones = await _context.Opcion
                .Where(o => _context.Pregunta.Any(p => p.id_encuesta == id_encuesta && p.id_pregunta == o.id_pregunta))
                .ToListAsync();

            if (opciones.Count == 0)
            {
                return NotFound("No se encontraron opciones para la encuesta proporcionada.");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Eliminar las opciones asociadas
                    _context.Opcion.RemoveRange(opciones);

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
                    return StatusCode(500, "Error al eliminar las opciones: " + ex.Message);
                }
            }

            return Ok("Opciones eliminadas con éxito.");
        }



        [HttpDelete("EliminarPreguntas/{id_encuesta}")]
        public async Task<IActionResult> EliminarPreguntas(int id_encuesta)
        {
            // Buscar las preguntas asociadas a la encuesta
            var preguntas = await _context.Pregunta
                .Where(p => p.id_encuesta == id_encuesta)
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
        public int id_estudiante { get; set; }  // ID del estudiante que está respondiendo
        public int id_pregunta { get; set; }    // ID de la pregunta que se está respondiendo
        public int id_opcion { get; set; }      // ID de la opción seleccionada
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
    {
        public string Titulo { get; set; }
        public List<PreguntaModel> Preguntas { get; set; } = new List<PreguntaModel>();
    }

    // Modelo para Pregunta con Opciones
    public class PreguntaModel
    {
        public string TextoPregunta { get; set; }
        public List<OpcionModel> Opciones { get; set; } = new List<OpcionModel>();
    }

    // Modelo para Opción
    public class OpcionModel
    {
        public string Texto { get; set; }
        public int Valor { get; set; }
    }
    public class OpcionDto
    {
        public int idOption { get; set; }
        public string Texto { get; set; }
        public int Valor { get; set; }
    }

    public class PreguntaConOpciones
    {
        public int IdPregunta { get; set; }
        public string TextoPregunta { get; set; }
        public List<OpcionDto> Opciones { get; set; }
    }

}