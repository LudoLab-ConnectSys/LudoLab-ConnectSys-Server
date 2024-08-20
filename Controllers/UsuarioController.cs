using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using LudoLab_ConnectSys_Server.Data;
using DocumentFormat.OpenXml.Math;


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



        /*[HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario, string tipoUsuario)
        {
            // Generar la contraseña temporal basada en la cédula del usuario
            string temporaryPassword = $"Lc@{usuario.cedula_usuario}";

            // Encriptar la contraseña utilizando BCrypt
            usuario.contrasena = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            // Asignar rol según el tipo de usuario
            var rol = await _context.Rol.FirstOrDefaultAsync(r => r.NombreRol == tipoUsuario);
            if (rol != null)
            {
                var usuarioRol = new UsuarioRol
                {
                    UsuarioId = usuario.id_usuario,
                    RolId = rol.RolId,
                    FechaAsignacion = DateTime.Now,
                    estadoActivo = true
                };
                _context.UsuarioRol.Add(usuarioRol);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("GetUsuario", new { id_usuario = usuario.id_usuario }, usuario);
        }*/

        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario, string tipoUsuario)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Generar la contraseña temporal basada en la cédula del usuario
                string temporaryPassword = $"Lc@{usuario.cedula_usuario}";

                // Encriptar la contraseña utilizando BCrypt
                usuario.contrasena = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

                _context.Usuario.Add(usuario);
                await _context.SaveChangesAsync();

                // Asignar rol según el tipo de usuario
                var rol = await _context.Rol.FirstOrDefaultAsync(r => r.NombreRol == tipoUsuario);
                if (rol != null)
                {
                    var usuarioRol = new UsuarioRol
                    {
                        UsuarioId = usuario.id_usuario,
                        RolId = rol.RolId,
                        FechaAsignacion = DateTime.Now,
                        estadoActivo = true
                    };
                    _context.UsuarioRol.Add(usuarioRol);
                    await _context.SaveChangesAsync();
                }

                // Confirmar la transacción
                await transaction.CommitAsync();

                return CreatedAtAction("GetUsuario", new { id_usuario = usuario.id_usuario }, usuario);
            }
            catch (Exception)
            {
                // Revertir la transacción en caso de fallo
                await transaction.RollbackAsync();
                return BadRequest("Error al crear el usuario.");
            }
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

        [HttpGet("Instructores")]
        public async Task<ActionResult<List<Usuario>>> GetInstructores()
        {
            var instructores = await _context.Usuario
                .Join(_context.Instructor,
                    u => u.id_usuario,
                    i => i.id_usuario,
                    (u, i) => u)
                .ToListAsync();

            return Ok(instructores);
        }
    }
}
