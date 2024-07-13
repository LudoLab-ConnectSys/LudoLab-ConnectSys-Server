using DirectorioDeArchivos.Shared;
using LudoLab_ConnectSys_Server.Data;
using Microsoft.EntityFrameworkCore;

public class GrupoService
{
    private readonly ApplicationDbContext _context;

    public GrupoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GrupoConDetalles>> CrearGruposAsync(CrearGruposParametros parametros)
    {
        // Obtener los estudiantes inscritos en el curso y periodo especificados
        var matriculas = await _context.Matricula
            .Where(m => m.id_curso == parametros.CursoId && m.id_periodo == parametros.PeriodoId)
            .ToListAsync();

        // Obtener los IDs de los estudiantes de las inscripciones
        var estudiantesIds = matriculas.Select(m => m.id_estudiante).ToList();

        // Obtener los estudiantes que no tienen grupo asignado
        var estudiantesSinGrupo = await _context.Estudiante
            .Where(e => estudiantesIds.Contains(e.id_estudiante) && e.id_grupo == null)
            .ToListAsync();

        // Obtener los horarios preferentes de estos estudiantes
        var horariosEstudiantes = await _context.HorarioPreferenteEstudiante
            .Where(h => estudiantesSinGrupo.Select(e => e.id_estudiante).Contains(h.id_estudiante))
            .ToListAsync();

        // Obtener los instructores registrados para este curso y periodo que no han alcanzado el número máximo de grupos
        var instructoresRegistrados = await _context.RegistroInstructor
            .Where(ri => ri.id_curso == parametros.CursoId && ri.id_periodo == parametros.PeriodoId && ri.id_grupo == null)
            .ToListAsync();

        var gruposCreados = new List<GrupoConDetalles>();

        // Inicializar el contador de nombres de grupos basado en los grupos existentes
        var ultimoGrupo = await _context.Grupo
            .Where(g => g.id_periodo == parametros.PeriodoId)
            .OrderByDescending(g => g.id_grupo)
            .Select(g => g.nombre_grupo)
            .FirstOrDefaultAsync();

        int contadorGrupo = 1;
        if (ultimoGrupo != null)
        {
            contadorGrupo = int.Parse(ultimoGrupo.Split('-')[1]) + 1;
        }

        foreach (var registro in instructoresRegistrados)
        {
            var horariosInstructor = await _context.HorarioPreferenteInstructor
                .Where(h => h.id_instructor == registro.id_instructor)
                .ToListAsync();

            var gruposPorInstructor = _context.Grupo.Count(g => g.id_instructor == registro.id_instructor);

            var idUsuarioInstructor = await _context.Instructor
                .Where(i => i.id_instructor == registro.id_instructor)
                .Select(i => i.id_usuario)
                .FirstOrDefaultAsync();

            var nombreInstructor = await _context.Usuario
                .Where(u => u.id_usuario == idUsuarioInstructor)
                .Select(u => u.nombre_usuario)
                .FirstOrDefaultAsync();

            foreach (var horarioInstructor in horariosInstructor)
            {
                var estudiantesCoincidentes = horariosEstudiantes
                    .Where(e =>
                        e.dia_semana == horarioInstructor.dia_semana &&
                        e.hora_inicio == horarioInstructor.hora_inicio &&
                        e.hora_fin == horarioInstructor.hora_fin &&
                        !_context.Estudiante.Any(est => est.id_estudiante == e.id_estudiante && est.id_grupo != null))
                    .Take(parametros.NumeroEstudiantesPorGrupo)
                    .ToList();

                if (estudiantesCoincidentes.Count() >= parametros.NumeroEstudiantesPorGrupo && gruposPorInstructor < parametros.NumeroMaximoGruposPorInstructor)
                {
                    var nombreGrupo = $"GR-{contadorGrupo}";

                    var estudiantesNombres = await _context.Estudiante
                        .Where(e => estudiantesCoincidentes.Select(ec => ec.id_estudiante).Contains(e.id_estudiante))
                        .Join(_context.Usuario, e => e.id_usuario, u => u.id_usuario, (e, u) => u.nombre_usuario)
                        .ToListAsync();

                    var grupoConDetalles = new GrupoConDetalles
                    {
                        id_grupo = contadorGrupo,
                        id_periodo = parametros.PeriodoId,
                        id_instructor = registro.id_instructor,
                        nombre_instructor = nombreInstructor,
                        nombre_grupo = nombreGrupo,
                        estudiantes = estudiantesNombres,
                        horarios = new List<HorarioGrupo>
                        {
                            new HorarioGrupo
                            {
                                dia_semana = horarioInstructor.dia_semana,
                                hora_inicio = horarioInstructor.hora_inicio,
                                hora_fin = horarioInstructor.hora_fin
                            }
                        }
                    };

                    gruposCreados.Add(grupoConDetalles);

                    // Remover estudiantes emparejados de la lista principal
                    horariosEstudiantes = horariosEstudiantes.Except(estudiantesCoincidentes).ToList();

                    // Incrementar el contador de nombres de grupos
                    contadorGrupo++;

                    // Incrementar el contador de grupos por instructor
                    gruposPorInstructor++;
                }
            }
        }

        return gruposCreados;
    }
}
