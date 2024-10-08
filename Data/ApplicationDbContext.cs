﻿using DirectorioDeArchivos.Shared;
using Microsoft.EntityFrameworkCore;
using LudoLab_ConnectSys_Server.Interceptors;

namespace LudoLab_ConnectSys_Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly AuditSaveChangesInterceptor _auditInterceptor;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, AuditSaveChangesInterceptor auditInterceptor)
            : base(options)
        {
            _auditInterceptor = auditInterceptor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
        }
        public DbSet<UploadResult> UploadResults => Set<UploadResult>();
        public DbSet<Certificado> Certificado { get; set; }
        public DbSet<Curso> Curso { get; set; }
        public DbSet<Periodo> Periodo { get; set; }
        public DbSet<PeriodoConNombreCurso> PeriodoConNombreCurso { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Estudiante> Estudiante { get; set; }
        public DbSet<Grupo> Grupo { get; set; }
        public DbSet<Instructor> Instructor { get; set; }
        public DbSet<Modalidad> Modalidad { get; set; }
        //public DbSet<Horas_instructor> Horas_instructor { get; set; }
        public DbSet<ListaPeriodo> ListaPeriodo { get; set; }
        public DbSet<UsuarioPeriodo> UsuarioPeriodo { get; set; }
        public DbSet<InstructorPeriodo> InstructorPeriodo { get; set; }
        public DbSet<HorarioPreferenteEstudiante> HorarioPreferenteEstudiante { get; set; }
        public DbSet<HorarioPreferenteInstructor> HorarioPreferenteInstructor { get; set; }
        public DbSet<Horario> Horario { get; set; }
        public DbSet<Matricula> Matricula { get; set; }
        public DbSet<RegistroInstructor> RegistroInstructor { get; set; }
        //public DbSet<GrupoDto> GrupoDto { get; set; }
        //public DbSet<InstructorDto> InstructorDto { get; set; }
        public DbSet<Encuesta> Encuesta { get; set; }//para las encuestas
        public DbSet<Pregunta> Pregunta { get; set; }
        public DbSet<Respuesta> Respuesta { get; set; }
        public DbSet<Horas_instructor> Horas_instructor { get; set; }
        public DbSet<UsuarioRol> UsuarioRol { get; set; }
        public DbSet <Rol> Rol { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la clave primaria compuesta para UsuarioRol
            modelBuilder.Entity<UsuarioRol>()
                .HasKey(ur => new { ur.UsuarioId, ur.RolId });
        }
        public DbSet<Opcion> Opcion { get; set; }
    }

    


}
