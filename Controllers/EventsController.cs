using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventsDemo.Infrastructure;
using EventsDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventsDemo.Controllers
{
    //[EnableCors("MSPolicy")] //This policy will apply to only this controller
    [Route("api/[controller]")]
    [Produces("text/json", "text/xml")] //Works like "Accept" header. Will return the response in only the specified type.
    //Can be applied at Action level as well

    [ApiController]
    public class EventsController : ControllerBase
    {
        private EventDbContext db;

        public EventsController(EventDbContext dbContext)
        {
            db = dbContext;
        }

        [HttpGet]
        public ActionResult<List<EventInfo>> GetEvents()
        {
            var events = db.Events.ToList();
            return Ok(events); //Returns with status code 200
        }

        /*//POST /api/events
        [HttpPost]
        //[HttpPost(Name ="AddEvent")]
        public ActionResult<EventInfo> AddEvent([FromBody]EventInfo eventInfo)
        {
            var entity = db.Events.Add(eventInfo);
            db.SaveChanges();
            //return Created("", entity.Entity); //Returns the status code 201           
            return CreatedAtAction(nameof(GetEvent), new { id = entity .Entity.Id}, entity.Entity);  //This adds location header to the response, which
            //gives the URL through which we can access the row passed
            //return CreatedAtRoute("GetById", new { id = entity.Entity.Id }, entity.Entity);  //This adds location header to the response, which
            //gives the URL through which we can access the row passed. Same thing can be done using CreatedAtRoute
            //method which takes the route name
        }*/


        //POST /api/events    
        [Authorize]
        [HttpPost("add")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<EventInfo>> AddEventAsync([FromBody]EventInfo eventInfo)
        {
            if (ModelState.IsValid)
            {
                var entity = await db.Events.AddAsync(eventInfo);
                await db.SaveChangesAsync();
                return CreatedAtRoute("GetByIdAsync", new { id = entity.Entity.Id }, entity.Entity);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /*//GET /api/events/{id}
        [HttpGet("{id}", Name = "GetById")] //This name is used by CreatedAtRoute() method
        public ActionResult<EventInfo> GetEvent([FromRoute]int id)
        {
            var eventInfo = db.Events.Find(id);
            return eventInfo;
        }*/

        //GET /api/events/{id}
        [HttpGet("{id}", Name = "GetByIdAsync")] //This name is used by CreatedAtRoute() method
        [ProducesResponseType((int)HttpStatusCode.OK)] //These attributes are used to give more data about the API for open specification
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EventInfo>> GetEventAsync([FromRoute]int id)
        {
            //throw new Exception("hello"); //Uncomment this when custom exception is to be tested
            var eventInfo = await db.Events.FindAsync(id);
            return eventInfo;
        }
    }
}