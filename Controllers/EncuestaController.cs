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
        [HttpGet("verificar-respuestas/{idEstudiante}")]//verificar si el estudiante ya respondió la encuesta
        public async Task<ActionResult<bool>> VerificarRespuestas(int idEstudiante)
        {
            var existeRespuesta = await _context.Respuesta
                .AnyAsync(r => r.id_estudiante == idEstudiante);

            return Ok(existeRespuesta);
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
    }
    public class RespuestaDto
    {
        public int id_estudiante { get; set; }
        public int id_pregunta { get; set; }
        public string texto_respuesta { get; set; }
    }
}