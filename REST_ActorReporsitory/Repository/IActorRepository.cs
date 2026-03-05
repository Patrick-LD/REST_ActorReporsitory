using REST_ActorReporsitory.Models;

namespace REST_ActorReporsitory.Repository
{
    public interface IActorRepository
    {
        Actor Add(Actor actor);
        Actor? Delete(int id);
        IEnumerable<Actor> GetAll(string? nameFilter = null, string? sortBy = null);
        Actor? GetById(int id);
        Actor? Update(int id, Actor updatedActor);
    }
}