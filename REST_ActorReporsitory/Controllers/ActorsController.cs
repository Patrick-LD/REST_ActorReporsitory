using Microsoft.AspNetCore.Mvc;
using REST_ActorReporsitory.Models;
using REST_ActorReporsitory.Repository;
using Microsoft.AspNetCore.Authorization;

namespace REST_ActorReporsitory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Ingen [Authorize] på klasse-niveau - så GET-endpoints er åbne for alle.
    // [Authorize] sættes kun på de enkelte metoder der kræver token (POST og DELETE)
    public class ActorsController : ControllerBase
    {
        private readonly IActorRepository _repo;

        // Constructor injection - ASP.NET giver os automatisk den registrerede IActorRepository
        public ActorsController(IActorRepository repo)
        {
            _repo = repo;
        }

        // GET /api/Actors - Hent alle actors (kræver IKKE token)
        // [FromQuery] læser parametre fra URL'en, f.eks. /api/Actors?nameFilter=Tom&sortBy=name
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
                    return NoContent(); // 204 - ingen data fundet
                }
                return Ok(actors); // 200 - returnerer listen af actors
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // 400 - ugyldigt input
            }
        }

        // GET /api/Actors/{id} - Hent én actor via ID (kræver IKKE token)
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Actor> Get(int id)
        {
            Actor? actor = _repo.GetById(id);
            if (actor == null)
            {
                return NotFound(); // 404 - actor med dette ID findes ikke
            }
            return Ok(actor);
        }

        // POST /api/Actors - Opret ny actor (kræver token pga. [Authorize])
        // Uden gyldigt token returneres 401 Unauthorized automatisk
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Actor> Post([FromBody] Actor actor)
        {
            try
            {
                Actor created = _repo.Add(actor);
                // 201 Created - returnerer den oprettede actor med location-header
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT /api/Actors/{id} - Opdater en actor (kræver IKKE token)
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

        // DELETE /api/Actors/{id} - Slet en actor (kræver token pga. [Authorize])
        // Uden gyldigt token returneres 401 Unauthorized automatisk
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

        // OPTIONS /api/Actors - Bruges af browsere til CORS preflight-requests
        [HttpOptions]
        public void Options()
        {

        }
    }
}
