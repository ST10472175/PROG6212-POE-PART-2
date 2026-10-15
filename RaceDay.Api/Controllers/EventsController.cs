using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [ApiController]
    [Route("api/events")]
    [Produces("application/json")]
    public class EventsController : BaseApiController
    {//start of class

        //object used to talk to the database
        private readonly RaceDayContext _db;

        public EventsController(RaceDayContext db)
        {//start of constructor
            _db = db;
        }//end of constructor

        ///View all events (Organisers and Participants).
        [HttpGet]
        [ProducesResponseType(typeof(List<EventResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEvents()
        {//start of method

            IActionResult blocked = CheckLoggedIn();
            if (blocked != null)
            {//start of if
                return blocked;
            }//end of if

            //get all the events from the database
            List<Event> events = await _db.Events
                .Include(e => e.Organiser)
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            //turn every event into the data we send back
            List<EventResponse> responses = new List<EventResponse>();
            foreach (Event ev in events)
            {//start of foreach
                responses.Add(MakeResponse(ev));
            }//end of foreach

            return Ok(responses);

        }//end of method

        //View one event by its id (Organisers and Participants)
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEvent(int id)
        {//start of method

            IActionResult blocked = CheckLoggedIn();
            if (blocked != null)
            {//start of if
                return blocked;
            }//end of if

            Event ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {//start of if
                return NotFound("Event not found.");
            }//end of if

            return Ok(MakeResponse(ev));

        }//end of method

       //Create a new event (Organisers only).
       
        [HttpPost]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateEvent(EventRequest request)
        {//start of method

            IActionResult blocked = CheckOrganiser();
            if (blocked != null)
            {//start of if
                return blocked;
            }//end of if

            string problem = CheckEventData(request);
            if (problem != "")
            {//start of if
                return BadRequest(problem);
            }//end of if

            //create the event and fill in the details
            Event ev = new Event();
            ev.EventName = request.EventName.Trim();
            ev.Description = request.Description.Trim();
            ev.EventDate = request.EventDate;
            ev.Location = request.Location.Trim();
            ev.Distance = request.Distance;
            ev.EventType = request.EventType;
            ev.OrganiserId = GetOrganiserId();

            //save to the database
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            //load the organiser so that the name is in the response
            await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, MakeResponse(ev));

        }//end of method

        //Update one of your own events (Organisers only).
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
        {//start of method

            IActionResult blocked = CheckOrganiser();
            if (blocked != null)
            {//start of if
                return blocked;
            }//end of if

            Event ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {//start of if
                return NotFound("Event not found.");
            }//end of if

            //an organiser can only change their own events
            if (ev.OrganiserId != GetOrganiserId())
            {//start of if
                return NotYourEvent();
            }//end of if

            string problem = CheckEventData(request);
            if (problem != "")
            {//start of if
                return BadRequest(problem);
            }//end of if

            //change the details
            ev.EventName = request.EventName.Trim();
            ev.Description = request.Description.Trim();
            ev.EventDate = request.EventDate;
            ev.Location = request.Location.Trim();
            ev.Distance = request.Distance;
            ev.EventType = request.EventType;

            await _db.SaveChangesAsync();
            return Ok(MakeResponse(ev));

        }//end of method

        //Delete one of your own events (Organisers only).
        
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEvent(int id)
        {//start of method

            IActionResult blocked = CheckOrganiser();
            if (blocked != null)
            {//start of if
                return blocked;
            }//end of if

            //load the event together with everything that belongs to it
            Event ev = await _db.Events
                .Include(e => e.Categories)
                .Include(e => e.Assignments)
                .Include(e => e.Results)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
            {//start of if
                return NotFound("Event not found.");
            }//end of if

            if (ev.OrganiserId != GetOrganiserId())
            {//start of if
                return NotYourEvent();
            }//end of if

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return NoContent();

        }//end of method

        //method to check the event data, it returns an empty text when everything is fine
        private string CheckEventData(EventRequest request)
        {//start of method

            if (request.EventDate == DateTime.MinValue)
            {//start of if
                return "Event date is required.";
            }//end of if

            if (Enum.IsDefined(typeof(EventType), request.EventType) == false)
            {//start of if
                return "Event type must be Run, Walk or Cycle.";
            }//end of if

            return "";

        }//end of method

        //method to turn the event into the data we send back
        private EventResponse MakeResponse(Event ev)
        {//start of method

            EventResponse response = new EventResponse();
            response.EventId = ev.EventId;
            response.EventName = ev.EventName;
            response.Description = ev.Description;
            response.EventDate = ev.EventDate;
            response.Location = ev.Location;
            response.Distance = ev.Distance;
            response.EventType = ev.EventType;
            response.OrganiserId = ev.OrganiserId;

            if (ev.Organiser != null)
            {//start of if
                response.OrganiserName = ev.Organiser.Name;
            }//end of if

            return response;

        }//end of method

    }//end of class
}//end of namespace
