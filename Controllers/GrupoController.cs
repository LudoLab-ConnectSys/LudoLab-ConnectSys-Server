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
                    horas_asignadas_estudiante = geu.ge.e.horas_asignadas_estudiante,
                    IdGrupo = geu.ge.g.id_grupo,
                    IdPeriodo = geu.ge.g.id_periodo,
                    IdInstructor = geu.ge.g.id_instructor,
                    NombreCertificado = c.nombre_certificado // Obtener el nombre del certificado

                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("GetInstructoresPorPeriodo/{id_periodo}")]
        public async Task<ActionResult<List<InstructorPeriodo>>> GetInstructoresPorPeriodo(int id_periodo)
        {
            var result = await (from i in _context.Instructor
                                join u in _context.Usuario on i.id_usuario equals u.id_usuario
                                join h in _context.Horas_instructor on i.id_instructor equals h.id_instructor
                                join p in _context.Periodo on h.id_periodo equals p.id_periodo
                                join lp in _context.ListaPeriodo on p.id_ListaPeriodo equals lp.id_lista_periodo
                                join c in _context.Curso on p.id_curso equals c.id_curso
                                join cert in _context.Certificado on p.id_periodo equals cert.id_periodo
                                where h.id_periodo == id_periodo
                                select new InstructorPeriodo
                                {
                                    IdUsuario = u.id_usuario,
                                    CedulaUsuario = u.cedula_usuario,
                                    NombreUsuario = u.nombre_usuario,
                                    ApellidosUsuario = u.apellidos_usuario,
                                    EdadUsuario = u.edad_usuario,
                                    CorreoUsuario = u.correo_usuario,
                                    CelularUsuario = u.celular_usuario,
                                    TelefonoUsuario = u.telefono_usuario,
                                    HorasGanadasInstructor = h.horas_ganadas_instructor,
                                    NombreCertificado = cert.nombre_certificado,
                                    NombreCurso = c.nombre_curso,
                                    NombrePeriodo = lp.nombre_periodo
                                }).ToListAsync();

            return Ok(result);
        }

    }
}
