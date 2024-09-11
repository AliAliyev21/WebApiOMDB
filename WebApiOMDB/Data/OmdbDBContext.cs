using Microsoft.EntityFrameworkCore;
using WebApiOMDB.Entities;

namespace WebApiOMDB.Data
{
    public class OmdbDBContext : DbContext
    {
        public OmdbDBContext(DbContextOptions<OmdbDBContext> options)
            :base(options) { }


        public DbSet<Movie> Movies { get; set; }

    }
}
