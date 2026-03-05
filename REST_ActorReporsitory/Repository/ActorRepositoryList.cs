using REST_ActorReporsitory.Models;

namespace REST_ActorReporsitory.Repository
{
    public class ActorRepositoryList : IActorRepository
    {
        private readonly List<Actor> actors = [];
        private int nextId = 1;

        public ActorRepositoryList(bool includeData = false)
        {
            if (includeData)
            {
                Add(new Actor { Name = "Tom Hanks", birthYear = 1956 });
                Add(new Actor { Name = "Meryl Streep", birthYear = 1949 });
                Add(new Actor { Name = "Leonardo DiCaprio", birthYear = 1974 });
            }
        }

        public IEnumerable<Actor> GetAll(string? nameFilter = null, string? sortBy = null)
        {
            IEnumerable<Actor> result = actors.AsReadOnly();
            if (!string.IsNullOrEmpty(nameFilter))
            {
                if (nameFilter.Length < 2)
                    throw new ArgumentException("nameFilter must be at least 2 characters");
                result = result.Where(a => a.Name != null && a.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                result = sortBy switch
                {
                    "name" => result.OrderBy(a => a.Name),
                    "name_desc" => result.OrderByDescending(a => a.Name),
                    "birthYear" => result.OrderBy(a => a.birthYear),
                    "birthYear_desc" => result.OrderByDescending(a => a.birthYear),
                    _ => throw new ArgumentException($"Invalid sortBy value: '{sortBy}'")
                };
            }
            return result;
        }

        public Actor Add(Actor actor)
        {
            ArgumentNullException.ThrowIfNull(actor);
            actor.Id = nextId++;
            actors.Add(actor);
            return actor;
        }

        public Actor? GetById(int id)
        {
            return actors.FirstOrDefault(a => a.Id == id);
        }

        public Actor? Delete(int id)
        {
            var actor = GetById(id);
            if (actor != null)
            {
                actors.Remove(actor);
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
                return existingActor;
            }
            return null;
        }
    }
}
