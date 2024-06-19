using LudoLab_ConnectSys_Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificadoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CertificadoController(ApplicationDbContext context)
        {

            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Certificado>>> GetCertificado()
        {
            var lista = await _context.Certificado.ToListAsync();
            return Ok(lista);
        }


        [HttpGet]
        [Route("{id_certificado}")]
        public async Task<ActionResult<List<Certificado>>> GetSingleCertificado(int id_certificado)
        {
            var miobjeto = await _context.Certificado.FirstOrDefaultAsync(ob => ob.id_certificado == id_certificado);
            if (miobjeto == null)
            {
                return NotFound(" :/");
            }

            return Ok(miobjeto);
        }

        [HttpPost]
        public async Task<ActionResult<Certificado>> CreateCertificado(Certificado objeto)
        {

            _context.Certificado.Add(objeto);
            await _context.SaveChangesAsync();
            return Ok(await GetDbCertificado());
        }

        [HttpPut("{id_certificado}")]
        public async Task<ActionResult<List<Certificado>>> UpdateCertificado(Certificado objeto)
        {

            var DbObjeto = await _context.Certificado.FindAsync(objeto.id_certificado);
            if (DbObjeto == null)
                return BadRequest("no se encuentra");
            DbObjeto.nombre_certificado = objeto.nombre_certificado;


            await _context.SaveChangesAsync();

            return Ok(await _context.Certificado.ToListAsync());


        }


        [HttpDelete]
        [Route("{id_certificado}")]
        public async Task<ActionResult<List<Certificado>>> DeleteCertificado(int id_certificado)
        {
            var DbObjeto = await _context.Certificado.FirstOrDefaultAsync(Ob => Ob.id_certificado == id_certificado);
            if (DbObjeto == null)
            {
                return NotFound("no existe :/");
            }

            _context.Certificado.Remove(DbObjeto);
            await _context.SaveChangesAsync();

            return Ok(await GetDbCertificado());
        }


        private async Task<List<Certificado>> GetDbCertificado()
        {
            return await _context.Certificado.ToListAsync();
        }

    }
}
