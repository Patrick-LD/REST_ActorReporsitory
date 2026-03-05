using Microsoft.EntityFrameworkCore;

namespace REST_ActorReporsitory.Models
{
    public class ActorDbContext : DbContext
    {
        public ActorDbContext(DbContextOptions<ActorDbContext> options) : base(options)
        {
        }
        public DbSet<Actor> Actors { get; set; }
    }
}
