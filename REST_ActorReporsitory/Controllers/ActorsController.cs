using Microsoft.AspNetCore.Mvc;
using REST_ActorReporsitory.Models;
using REST_ActorReporsitory.Repository;
using Microsoft.AspNetCore.Authorization;

namespace REST_ActorReporsitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly IActorRepository _repo;

        public ActorsController(IActorRepository repo)
        {
            _repo = repo;
        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public ActionResult<IEnumerable<Actor>> Get([FromQuery] string? nameFilter = null, [FromQuery] string? sortBy = null)
        {
            try
            {
                var actors = _repo.GetAll(nameFilter, sortBy);
                if (!actors.Any())
                {
                    return NoContent();
                }
                return Ok(actors);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<Actor> Get(int id)
        {
            Actor? actor = _repo.GetById(id);
            if (actor == null)
            {
                return NotFound();
            }
            return Ok(actor);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Actor> Post([FromBody] Actor actor)
        {
            try
            {
                Actor created = _repo.Add(actor);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<Actor> Put(int id, [FromBody] Actor actor)
        {
            Actor? updated = _repo.Update(id, actor);
            if (updated == null)
            {
                return NotFound();
            }
            return Ok(updated);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public ActionResult<Actor> Delete(int id)
        {
            Actor? deleted = _repo.Delete(id);
            if (deleted == null)
            {
                return NotFound();
            }
            return Ok(deleted);
        }
        [HttpOptions]
        public void Options()
        {

        }
    }
}
