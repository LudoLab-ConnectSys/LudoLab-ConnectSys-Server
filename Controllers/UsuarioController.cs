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
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuario.ToListAsync();
        }

        // GET: api/Usuario/{id_usuario}
        [HttpGet("{id_usuario}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id_usuario)
        {
            var usuario = await _context.Usuario.FindAsync(id_usuario);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // POST: api/Usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id_usuario = usuario.id_usuario }, usuario);
        }

        // PUT: api/Usuario/{id_usuario}
        [HttpPut("{id_usuario}")]
        public async Task<IActionResult> PutUsuario(int id_usuario, Usuario usuario)
        {
            if (id_usuario != usuario.id_usuario)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id_usuario))
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

        // DELETE: api/Usuario/{id_usuario}
        [HttpDelete("{id_usuario}")]
        public async Task<IActionResult> DeleteUsuario(int id_usuario)
        {
            var usuario = await _context.Usuario.FindAsync(id_usuario);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuario.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id_usuario)
        {
            return _context.Usuario.Any(e => e.id_usuario == id_usuario);
        }
    }
}
