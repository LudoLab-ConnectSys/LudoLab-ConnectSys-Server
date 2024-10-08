﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace LudoLab_ConnectSys_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CursoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CursoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Curso>>> GetCursos()
        {
            return await _context.Curso.ToListAsync();
        }

        [HttpGet("{id_curso}")]
        public async Task<ActionResult<Curso>> GetCurso(int id_curso)
        {
            var curso = await _context.Curso.FirstOrDefaultAsync(c => c.id_curso == id_curso);
            if (curso == null)
            {
                return NotFound();
            }
            return curso;
        }

        [HttpPost]
        public async Task<ActionResult<Curso>> CreateCurso(Curso curso)
        {
            _context.Curso.Add(curso);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCurso), new { id_curso = curso.id_curso }, curso);
        }

        [HttpPut("{id_curso}")]
        public async Task<IActionResult> UpdateCurso(int id_curso, Curso curso)
        {
            if (id_curso != curso.id_curso)
            {
                return BadRequest();
            }

            _context.Entry(curso).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(id_curso))
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

        [HttpDelete("{id_curso}")]
        public async Task<IActionResult> DeleteCurso(int id_curso)
        {
            // Verificar si el curso existe
            var curso = await _context.Curso.FindAsync(id_curso);
            if (curso == null)
            {
                return NotFound(new { message = "Curso no encontrado" });
            }

            // Verificar dependencias: Periodos y Estudiantes
            var tienePeriodos = await _context.Periodo.AnyAsync(p => p.id_curso == id_curso);
            var tieneEstudiantes = await _context.Matricula.AnyAsync(m => m.id_curso == id_curso);

            if (tienePeriodos || tieneEstudiantes)
            {
                return Conflict(new { message = "No es posible eliminar el curso porque tiene registros asociados en otras tablas (periodos o estudiantes)." });
            }

            try
            {
                _context.Curso.Remove(curso);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Manejo general de errores
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al eliminar el curso: {ex.Message}");
            }
        }

        [HttpGet("CursosConPeriodosActivos")]
        public async Task<ActionResult<List<CursoConPeriodoActivo>>> GetCursosConPeriodosActivos()
        {
            var query = from c in _context.Curso
                        join p in _context.Periodo on c.id_curso equals p.id_curso
                        where p.activo == true
                        group c by new { c.id_curso, c.nombre_curso } into cursoGroup
                        select new CursoConPeriodoActivo
                        {
                            id_curso = cursoGroup.Key.id_curso,
                            nombre_curso = cursoGroup.Key.nombre_curso,
                        };

            var cursos = await query.Distinct().ToListAsync();
            return Ok(cursos);
        }

        [HttpGet("{id_grupo}/detalles")]
        public async Task<ActionResult<GrupoConDetalles>> GetGrupoDetalles(int id_grupo)
        {
            var grupo = await (from g in _context.Grupo
                               join i in _context.Instructor on g.id_instructor equals i.id_instructor
                               join u in _context.Usuario on i.id_usuario equals u.id_usuario
                               where g.id_grupo == id_grupo
                               select new GrupoConDetalles
                               {
                                   id_grupo = g.id_grupo,
                                   id_periodo = g.id_periodo,
                                   id_instructor = g.id_instructor,
                                   nombre_instructor = u.nombre_usuario + " " + u.apellidos_usuario,
                                   nombre_grupo = g.nombre_grupo ?? string.Empty,
                                   estudiantes = (from e in _context.Estudiante
                                                  join ue in _context.Usuario on e.id_usuario equals ue.id_usuario
                                                  where e.id_grupo == g.id_grupo
                                                  select ue.nombre_usuario + " " + ue.apellidos_usuario).ToList()
                               }).FirstOrDefaultAsync();

            if (grupo == null)
            {
                return NotFound();
            }

            return Ok(grupo);
        }


        [HttpGet("{id_curso}/Nombre")]
        public async Task<ActionResult<string>> GetNombreCurso(int id_curso)
        {
            var curso = await _context.Curso
                                      .Where(c => c.id_curso == id_curso)
                                      .Select(c => c.nombre_curso)
                                      .FirstOrDefaultAsync();

            if (curso == null)
            {
                return NotFound();
            }

            return Ok(curso);
        }

        private bool CursoExists(int id_curso)
        {
            return _context.Curso.Any(e => e.id_curso == id_curso);
        }

       
    }
}
