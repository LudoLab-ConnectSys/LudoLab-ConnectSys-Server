using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;
using DirectorioDeArchivos.Shared;
using System.Collections.Generic;

namespace LudoLab_ConnectSys_Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<UploadResult> UploadResults => Set<UploadResult>();
        public DbSet<Certificado> Certificado { get; set; }
        public DbSet<Curso> Curso { get; set; }
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<PeriodoConNombreCurso> PeriodoConNombreCurso { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Estudiante> Estudiante { get; set; }
        public DbSet<Grupo> Grupo { get; set; }
        public DbSet<Horas_instructor> Horas_instructor { get; set; }
        public DbSet<Instructor> Instructor { get; set; }
        public DbSet<ListaPeriodo> ListaPeriodo { get; set; }
        public DbSet<UsuarioPeriodo> UsuarioPeriodo { get; set; }
        public DbSet<InstructorPeriodo> InstructorPeriodo { get; set; }
    }
}
