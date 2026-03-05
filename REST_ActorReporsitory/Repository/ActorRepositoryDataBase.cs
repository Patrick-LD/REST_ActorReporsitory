using REST_ActorReporsitory.Models;

namespace REST_ActorReporsitory.Repository
{
    public class ActorRepositoryDataBase : IActorRepository
    {
        private readonly ActorDbContext _context;

        public ActorRepositoryDataBase(ActorDbContext context)
        {
            _context = context;
        }

        public Actor Add(Actor actor)
        {
            if (actor is null)
            {
                throw new ArgumentNullException(nameof(actor));
            }
            _context.Actors.Add(actor);
            _context.SaveChanges();
            return actor;
        }

        public IEnumerable<Actor> GetAll(string? nameFilter = null, string? sortBy = null)
        {
            IQueryable<Actor> result = _context.Actors;
            if (!string.IsNullOrEmpty(nameFilter))
            {
                result = result.Where(a => a.Name != null && a.Name.Contains(nameFilter));
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                result = sortBy switch
                {
                    "name" => result.OrderBy(a => a.Name),
                    "name_desc" => result.OrderByDescending(a => a.Name),
                    "birthYear" => result.OrderBy(a => a.birthYear),
                    "birthYear_desc" => result.OrderByDescending(a => a.birthYear),
                    _ => result
                };
            }
            return result;
        }

        public Actor? GetById(int id)
        {
            return _context.Actors.Find(id);
        }

        public Actor? Delete(int id)
        {
            var actor = GetById(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                _context.SaveChanges();
                return actor;
            }
            return null;
        }

        public Actor? Update(int id, Actor updatedActor)
        {
            var existingActor = GetById(id);
            if (existingActor != null)
            {
                existingActor.Name = updatedActor.Name;
                existingActor.birthYear = updatedActor.birthYear;
                _context.SaveChanges();
                return existingActor;
            }
            return null;
        }
    }
}
