using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrupoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GrupoController(ApplicationDbContext context)
        {

            _context = context;
        }

        [HttpGet]
        [Route("{id_periodo}")]
        public async Task<ActionResult<List<UsuarioPeriodo>>> GetUsuariosByPeriodo(int id_periodo)
        {
            var result = await _context.Grupo
                .Where(g => g.id_periodo == id_periodo)
                .Join(_context.Estudiante, g => g.id_grupo, e => e.id_grupo, (g, e) => new { g, e })
                .Join(_context.Usuario, ge => ge.e.id_usuario, u => u.id_usuario, (ge, u) => new { ge, u })
                .Join(_context.Certificado, geu => geu.ge.g.id_periodo, c => c.id_periodo, (geu, c) => new UsuarioPeriodo // Join con Certificado
                {
                    IdUsuario = geu.u.id_usuario,
                    CedulaUsuario = geu.u.cedula_usuario,
                    NombreUsuario = geu.u.nombre_usuario,
                    ApellidosUsuario = geu.u.apellidos_usuario,
                    EdadUsuario = geu.u.edad_usuario,
                    CorreoUsuario = geu.u.correo_usuario,
                    CelularUsuario = geu.u.celular_usuario,
                    TelefonoUsuario = geu.u.telefono_usuario,
                    IdGrupo = geu.ge.g.id_grupo,
                    IdPeriodo = geu.ge.g.id_periodo,
                    IdInstructor = geu.ge.g.id_instructor,
                    NombreCertificado = c.nombre_certificado // Obtener el nombre del certificado
                })
                .ToListAsync();

            return Ok(result);
        }

    }
}
