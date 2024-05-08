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
    }
}
