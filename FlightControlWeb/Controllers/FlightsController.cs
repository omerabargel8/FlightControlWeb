using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private IFlightsManager flightsManager;
        public FlightsController(IFlightsManager flightsManager)
        {
            this.flightsManager = flightsManager;
        }
        // GET: api/Flights?relative_to=<DATE_TIME>
        [HttpGet]
        public IEnumerable<Flight> GetAllFlights(string relative_to)
        {
            string request = Request.QueryString.Value;
            bool isExternals = request.Contains("sync_all");
            return flightsManager.getAllFlights(relative_to, isExternals);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            flightsManager.deleteFlight(id);
        }

    }
}
