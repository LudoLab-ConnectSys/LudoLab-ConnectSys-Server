using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListaPeriodoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ListaPeriodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ListaPeriodo>>> GetListaPeriodos()
        {
            return await _context.ListaPeriodo.ToListAsync();
        }

        [HttpGet("{id_lista_periodo}")]
        public async Task<ActionResult<ListaPeriodo>> GetListaPeriodo(int id_lista_periodo)
        {
            var listaPeriodo = await _context.ListaPeriodo.FirstOrDefaultAsync(lp => lp.id_lista_periodo == id_lista_periodo);
            if (listaPeriodo == null)
            {
                return NotFound();
            }
            return listaPeriodo;
        }

        [HttpPost]
        public async Task<ActionResult<ListaPeriodo>> CreateListaPeriodo(ListaPeriodo listaPeriodo)
        {
            _context.ListaPeriodo.Add(listaPeriodo);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetListaPeriodo), new { id_lista_periodo = listaPeriodo.id_lista_periodo }, listaPeriodo);
        }

        [HttpPut("{id_lista_periodo}")]
        public async Task<IActionResult> UpdateListaPeriodo(int id_lista_periodo, ListaPeriodo listaPeriodo)
        {
            if (id_lista_periodo != listaPeriodo.id_lista_periodo)
            {
                return BadRequest();
            }

            _context.Entry(listaPeriodo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ListaPeriodoExists(id_lista_periodo))
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

        [HttpDelete("{id_lista_periodo}")]
        public async Task<IActionResult> DeleteListaPeriodo(int id_lista_periodo)
        {
            var listaPeriodo = await _context.ListaPeriodo.FindAsync(id_lista_periodo);
            if (listaPeriodo == null)
            {
                return NotFound();
            }

            _context.ListaPeriodo.Remove(listaPeriodo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ListaPeriodoExists(int id_lista_periodo)
        {
            return _context.ListaPeriodo.Any(e => e.id_lista_periodo == id_lista_periodo);
        }
    }
}
