using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FlightControlWeb.Models;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private IFlightsManager flightsManager = new FlightsManager();
        public FlightPlanController(IFlightsManager flightsManager)
        {
            this.flightsManager = flightsManager;
        }

        /**
        // GET: api/FlightPlan
        [HttpGet]
        public IEnumerable<FlightPlan> Get()
        {
            return flightsManager.getAllFlightPlans();
        }*/

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "Get")]
        public FlightPlan GetFlightPlanById(string id)
        {
            string request = Request.QueryString.Value;
            bool internRequest = request.Contains("?");
            return flightsManager.GetFlightPlanById(id, internRequest);
        }

        // POST: api/FlightPlan
        [HttpPost]
        public void Post(FlightPlan fp)
        {
            flightsManager.addFlightPlan(fp);
        }

        // PUT: api/FlightPlan/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
