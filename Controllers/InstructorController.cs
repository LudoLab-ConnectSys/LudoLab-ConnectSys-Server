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
    }
}
