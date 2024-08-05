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
            var periodo = await _context.Periodo.FindAsync(idPeriodo);
            if (periodo == null)
            {
                return NotFound();
            }

            var curso = await _context.Curso.FindAsync(periodo.id_curso);
            var grupos = await _context.Grupo.Where(g => g.id_periodo == idPeriodo).ToListAsync();
            var horarios = await _context.Horario.Where(h => grupos.Select(g => g.id_grupo).Contains(h.id_grupo)).ToListAsync();
            var horasInstructores = await _context.Horas_instructor.Where(h => h.id_periodo == idPeriodo).ToListAsync();
            var instructores = await _context.Instructor.Where(i => grupos.Select(g => g.id_instructor).Contains(i.id_instructor)).ToListAsync();
            var estudiantes = await _context.Estudiante.Where(e => grupos.Select(g => g.id_grupo).Contains(e.id_grupo.Value)).ToListAsync();
            var usuarios = await _context.Usuario.Where(u => instructores.Select(i => i.id_usuario).Contains(u.id_usuario)
                || estudiantes.Select(e => e.id_usuario).Contains(u.id_usuario)).ToListAsync();

            var reporte = new ReporteCursoDTO
            {
                Periodo = periodo,
                Curso = curso,
                Grupos = grupos,
                Horarios = horarios,
                HorasInstructores = horasInstructores,
                Instructores = instructores,
                Estudiantes = estudiantes,
                Usuarios = usuarios
            };

            return Ok(reporte);
        }
    }
    public class ReporteCursoDTO
    {
        public Periodo Periodo { get; set; }
        public Curso Curso { get; set; }
        public List<Grupo> Grupos { get; set; }
        public List<Horario> Horarios { get; set; }
        public List<Horas_instructor> HorasInstructores { get; set; }
        public List<Instructor> Instructores { get; set; }
        public List<Estudiante> Estudiantes { get; set; }
        public List<Usuario> Usuarios { get; set; }
    }

}
