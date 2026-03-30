using BlogProjesi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogProjesi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Makale> Makaleler { get; set; }
    }
}
