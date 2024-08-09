using LudoLab_ConnectSys_Server.Data;
using DirectorioDeArchivos.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using LudoLab_ConnectSys_Server.Data;
using DirectorioDeArchivos.Shared;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Pdf.Canvas;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using System.Text;
using System.IO;
using System.Linq;


namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public FileController(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        [HttpPost]//cargar archivos
        public async Task<ActionResult<List<UploadResult>>> UploadFile(List<IFormFile> files)
        {
            Console.WriteLine("enviar a uploads");
            List<UploadResult> uploadResults = new List<UploadResult>();

            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;
                uploadResult.Nombre = untrustedFileName;
                //var trustedFileNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);

                trustedFileNameForFileStorage = file.FileName;
                var path = Path.Combine(_env.ContentRootPath, "uploads", trustedFileNameForFileStorage);
                Console.WriteLine("enviar a uploads");

                //await using FileStream fs = new(path, FileMode.Create);
                //await file.CopyToAsync(fs);
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }

                uploadResult.StoredFileName = trustedFileNameForFileStorage;
                uploadResult.ContentType = file.ContentType;
                uploadResults.Add(uploadResult);

                // Insertar texto en el PDF
                InsertTextInPdf(path, "Nombre de la Institución", "Texto del Certificado");

                _context.UploadResults.Add(uploadResult);
                //await _context.SaveChangesAsync();
            }

            //await _context.SaveChangesAsync();

            return Ok(uploadResults);
        }
        private void InsertTextInPdf(string filePath, string institutionName, string certificateText)
        {
            Console.WriteLine($"path file: {filePath}");
            try
            {
                using (var pdfReader = new PdfReader(filePath))
                using (var pdfWriter = new PdfWriter(filePath))
                using (var pdfDocument = new PdfDocument(pdfReader, pdfWriter))
                {
                    var document = new Document(pdfDocument);

                    var firstPage = pdfDocument.GetFirstPage();
                    var pdfCanvas = new PdfCanvas(firstPage);
                    var pageSize = firstPage.GetPageSize();

                    PdfFont font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                    pdfCanvas.BeginText()
                             .SetFontAndSize(font, 18)
                             .MoveText(pageSize.GetWidth() / 2 - 100, pageSize.GetHeight() - 50)
                             .ShowText(institutionName)
                             .EndText();

                    pdfCanvas.BeginText()
                             .SetFontAndSize(font, 12)
                             .MoveText(pageSize.GetWidth() / 2 - 100, pageSize.GetHeight() - 80)
                             .ShowText(certificateText)
                             .EndText();

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting text in PDF: {ex.Message}");
            }
        }



        [HttpGet("{fileName}")]//deswcargar archvos segun el nombre 
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            Console.WriteLine($"Download file {fileName}");
            //var uploadResult = await _context.UploadResults.FirstOrDefaultAsync(u => u.StoredFileName.Equals(fileName));

            var path = Path.Combine(_env.ContentRootPath, "uploads", fileName);
            // Verifica si el archivo existe
            if (!System.IO.File.Exists(path))
            {
                return NotFound(); // Devuelve un 404 si el archivo no se encuentra
            }
            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            // Determina el tipo de contenido basado en la extensión del archivo
            string contentType;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            Console.WriteLine($"esta es la extencion {extension}");

            switch (extension)
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
                default:
                    contentType = "application/octet-stream"; // Tipo de contenido genérico
                    break;
            }
            Console.WriteLine($"contentType is: {contentType}");
            Console.WriteLine($"path is: {path}");
            //return new FileContentResult(memory.ToArray(), "application/pdf");
            // Devuelve el archivo con el tipo de contenido adecuado
            return File(memory, contentType, Path.GetFileName(path));

            //return File(memory, "image/png", Path.GetFileName(path));//para imagenes pnd
            //return File(memory.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(path));//Para excel
            //return File(memory.ToArray(), "application/pdf", Path.GetFileName(path));//Para pdf
        }

        [HttpPut]
        [Route("ActualizarNombreArchivo")]//actualizar nombre de archvo en tabla certificado
        public async Task<IActionResult> ActualizarNombreArchivo([FromBody] ActualizacionNombreArchivoModel model)
        {
            var certificado = await _context.Certificado.FirstOrDefaultAsync(c => c.id_periodo == model.IdPeriodo);
            if (certificado != null)
            {
                certificado.nombre_certificado = model.NuevoNombreArchivo;
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        public class ActualizacionNombreArchivoModel
        {
            public int IdPeriodo { get; set; }
            public string NuevoNombreArchivo { get; set; }
        }
        /*------------------------------------- reportes de cursos completos -------------------------------------*/
        [HttpGet("GetReporte/{idPeriodo}")]
        public async Task<ActionResult<ReporteCursoDTO>> GetReporte(int idPeriodo)
        {
            var periodo = await _context.Periodo.FirstOrDefaultAsync(p => p.id_periodo == idPeriodo);

            if (periodo == null)
            {
                return NotFound();
            }

            var curso = await _context.Curso.FirstOrDefaultAsync(c => c.id_curso == periodo.id_curso);
            var listaPeriodo = await _context.ListaPeriodo.FirstOrDefaultAsync(lp => lp.id_lista_periodo == periodo.id_ListaPeriodo);

            var grupos = await _context.Grupo
                .Where(g => g.id_periodo == idPeriodo)
                .ToListAsync();

            var horarios = await _context.Horario
                .Where(h => grupos.Select(g => g.id_grupo).Contains(h.id_grupo))
                .ToListAsync();

            var horasInstructores = await _context.Horas_instructor
                .Where(h => h.id_periodo == idPeriodo)
                .ToListAsync();

            var instructoresIds = grupos.Select(g => g.id_instructor).ToList();
            var instructores = await _context.Instructor
                .Where(i => instructoresIds.Contains(i.id_instructor))
                .ToListAsync();

            var estudiantes = await _context.Estudiante
                .Where(e => grupos.Select(g => g.id_grupo).Contains(e.id_grupo.Value))
                .ToListAsync();

            var usuariosIds = instructores.Select(i => i.id_usuario).Concat(estudiantes.Select(e => e.id_usuario)).ToList();
            var usuarios = await _context.Usuario
                .Where(u => usuariosIds.Contains(u.id_usuario))
                .ToListAsync();

            var reporte = new ReporteCursoDTO
            {
                Periodo = new PeriodoDTO
                {
                    Id = periodo.id_periodo,
                    FechaInicio = periodo.fecha_inicio_periodo,
                    FechaFin = periodo.fecha_fin_periodo,
                    Activo = periodo.activo,
                    CursoNombre = curso?.nombre_curso,
                    ListaPeriodoNombre = listaPeriodo?.nombre_periodo
                },
                Curso = new CursoDTO
                {
                    Id = curso.id_curso,
                    Nombre = curso.nombre_curso,
                    Tipo = curso.tipo_curso,
                    Horas = curso.horas
                },
                Grupos = grupos.Select(g => new GrupoDTO
                {
                    Id = g.id_grupo,
                    Nombre = g.nombre_grupo,
                    InstructorNombre = instructores.FirstOrDefault(i => i.id_instructor == g.id_instructor)?.id_usuario.ToString()
                }).ToList(),
                Horarios = horarios.Select(h => new HorarioDTO
                {
                    Id = h.id_horario,
                    DiaSemana = h.dia_semana,
                    HoraInicio = h.hora_inicio,
                    HoraFin = h.hora_fin
                }).ToList(),
                HorasInstructores = horasInstructores.Select(h => new HorasInstructorDTO
                {
                    Id = h.id_horas_instructor,
                    InstructorNombre = instructores.FirstOrDefault(i => i.id_instructor == h.id_instructor)?.id_usuario.ToString(),
                    HorasGanadas = h.horas_ganadas_instructor
                }).ToList(),
                Instructores = instructores.Select(i => new InstructorDTO
                {
                    Id = i.id_instructor,
                    Nombre = usuarios.FirstOrDefault(u => u.id_usuario == i.id_usuario)?.nombre_usuario
                }).ToList(),
                Estudiantes = estudiantes.Select(e => new EstudianteDTO
                {
                    Id = e.id_estudiante,
                    Nombre = usuarios.FirstOrDefault(u => u.id_usuario == e.id_usuario)?.nombre_usuario,
                    Tipo = e.tipo_estudiante,
                    HorasAsignadas = e.horas_asignadas_estudiante
                }).ToList(),
                Usuarios = usuarios.Select(u => new UsuarioDTO
                {
                    Id = u.id_usuario,
                    Cedula = u.cedula_usuario,
                    Nombre = u.nombre_usuario,
                    Apellidos = u.apellidos_usuario,
                    Edad = u.edad_usuario,
                    Correo = u.correo_usuario,
                    Celular = u.celular_usuario,
                    Telefono = u.telefono_usuario
                }).ToList()
            };

            return Ok(reporte);
        }

        [HttpGet("GetReporteFilter/{idPeriodo}")]
        public async Task<ActionResult<ReporteCursoDTO>> GetReporteFilter(int idPeriodo, bool incluirTutores = true, bool incluirEstudiantes = true)
        {
            var periodo = await _context.Periodo.FirstOrDefaultAsync(p => p.id_periodo == idPeriodo);

            if (periodo == null)
            {
                return NotFound();
            }

            var curso = await _context.Curso.FirstOrDefaultAsync(c => c.id_curso == periodo.id_curso);
            var listaPeriodo = await _context.ListaPeriodo.FirstOrDefaultAsync(lp => lp.id_lista_periodo == periodo.id_ListaPeriodo);

            var grupos = await _context.Grupo
                .Where(g => g.id_periodo == idPeriodo)
                .ToListAsync();

            var horarios = await _context.Horario
                .Where(h => grupos.Select(g => g.id_grupo).Contains(h.id_grupo))
                .ToListAsync();

            var horasInstructores = await _context.Horas_instructor
                .Where(h => h.id_periodo == idPeriodo)
                .ToListAsync();

            List<Instructor> instructores = new List<Instructor>();
            List<Estudiante> estudiantes = new List<Estudiante>();
            List<Usuario> usuarios = new List<Usuario>();

            if (incluirTutores)
            {
                var instructoresIds = grupos.Select(g => g.id_instructor).ToList();
                instructores = await _context.Instructor
                    .Where(i => instructoresIds.Contains(i.id_instructor))
                    .ToListAsync();

                var instructoresUsuariosIds = instructores.Select(i => i.id_usuario).ToList();
                usuarios.AddRange(await _context.Usuario
                    .Where(u => instructoresUsuariosIds.Contains(u.id_usuario))
                    .ToListAsync());
            }

            if (incluirEstudiantes)
            {
                estudiantes = await _context.Estudiante
                    .Where(e => grupos.Select(g => g.id_grupo).Contains(e.id_grupo.Value))
                    .ToListAsync();

                var estudiantesUsuariosIds = estudiantes.Select(e => e.id_usuario).ToList();
                usuarios.AddRange(await _context.Usuario
                    .Where(u => estudiantesUsuariosIds.Contains(u.id_usuario))
                    .ToListAsync());
            }

            var reporte = new ReporteCursoDTO
            {
                Periodo = new PeriodoDTO
                {
                    Id = periodo.id_periodo,
                    FechaInicio = periodo.fecha_inicio_periodo,
                    FechaFin = periodo.fecha_fin_periodo,
                    Activo = periodo.activo,
                    CursoNombre = curso?.nombre_curso,
                    ListaPeriodoNombre = listaPeriodo?.nombre_periodo
                },
                Curso = new CursoDTO
                {
                    Id = curso.id_curso,
                    Nombre = curso.nombre_curso,
                    Tipo = curso.tipo_curso,
                    Horas = curso.horas
                },
                Grupos = grupos.Select(g => new GrupoDTO
                {
                    Id = g.id_grupo,
                    Nombre = g.nombre_grupo,
                    InstructorNombre = instructores.FirstOrDefault(i => i.id_instructor == g.id_instructor)?.id_usuario.ToString()
                }).ToList(),
                Horarios = horarios.Select(h => new HorarioDTO
                {
                    Id = h.id_horario,
                    DiaSemana = h.dia_semana,
                    HoraInicio = h.hora_inicio,
                    HoraFin = h.hora_fin
                }).ToList(),
                HorasInstructores = horasInstructores.Select(h => new HorasInstructorDTO
                {
                    Id = h.id_horas_instructor,
                    InstructorNombre = instructores.FirstOrDefault(i => i.id_instructor == h.id_instructor)?.id_usuario.ToString(),
                    HorasGanadas = h.horas_ganadas_instructor
                }).ToList(),
                Instructores = incluirTutores ? instructores.Select(i => new InstructorDTO
                {
                    Id = i.id_instructor,
                    Nombre = usuarios.FirstOrDefault(u => u.id_usuario == i.id_usuario)?.nombre_usuario
                }).ToList() : new List<InstructorDTO>(),
                Estudiantes = incluirEstudiantes ? estudiantes.Select(e => new EstudianteDTO
                {
                    Id = e.id_estudiante,
                    Nombre = usuarios.FirstOrDefault(u => u.id_usuario == e.id_usuario)?.nombre_usuario,
                    Tipo = e.tipo_estudiante,
                    HorasAsignadas = e.horas_asignadas_estudiante
                }).ToList() : new List<EstudianteDTO>(),
                Usuarios = usuarios.Distinct().Select(u => new UsuarioDTO
                {
                    Id = u.id_usuario,
                    Cedula = u.cedula_usuario,
                    Nombre = u.nombre_usuario,
                    Apellidos = u.apellidos_usuario,
                    Edad = u.edad_usuario,
                    Correo = u.correo_usuario,
                    Celular = u.celular_usuario,
                    Telefono = u.telefono_usuario
                }).ToList()
            };

            return Ok(reporte);
        }


    }
    public class ReporteCursoDTO
    {
        public PeriodoDTO Periodo { get; set; }
        public CursoDTO Curso { get; set; }
        public List<GrupoDTO> Grupos { get; set; }
        public List<HorarioDTO> Horarios { get; set; }
        public List<HorasInstructorDTO> HorasInstructores { get; set; }
        public List<InstructorDTO> Instructores { get; set; }
        public List<EstudianteDTO> Estudiantes { get; set; }
        public List<UsuarioDTO> Usuarios { get; set; }
    }

    public class PeriodoDTO
    {
        public int Id { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public bool Activo { get; set; }
        public string CursoNombre { get; set; }
        public string ListaPeriodoNombre { get; set; }
    }

    public class CursoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int Horas { get; set; }
    }

    public class GrupoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string InstructorNombre { get; set; }
    }

    public class HorarioDTO
    {
        public int Id { get; set; }
        public string DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }

    public class HorasInstructorDTO
    {
        public int Id { get; set; }
        public string InstructorNombre { get; set; }
        public int HorasGanadas { get; set; }
    }

    public class InstructorDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class EstudianteDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int HorasAsignadas { get; set; }
    }

    public class UsuarioDTO
    {
        public int Id { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public int Edad { get; set; }
        public string Correo { get; set; }
        public string Celular { get; set; }
        public string Telefono { get; set; }
    }
}
