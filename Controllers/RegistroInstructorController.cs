using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Mvc;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistroInstructorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistroInstructorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<RegistroInstructor>> PostRegistroInstructor(RegistroInstructor registroInstructor)
        {
            // Validar si el periodo existe
            var periodo = await _context.Periodo.FindAsync(registroInstructor.id_periodo);
            if (periodo == null)
            {
                return BadRequest("El periodo no existe.");
            }

            _context.RegistroInstructor.Add(registroInstructor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRegistroInstructor), new { id = registroInstructor.id_registro_instructor }, registroInstructor);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroInstructor>> GetRegistroInstructor(int id)
        {
            var registroInstructor = await _context.RegistroInstructor.FindAsync(id);

            if (registroInstructor == null)
            {
                return NotFound();
            }

            return registroInstructor;
        }
    }

}
