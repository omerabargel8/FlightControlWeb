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
        //private IFlightsManager flightsManager = new FlightsManager();
        private IFlightsManager flightsManager;
        public FlightPlanController(IFlightsManager flightsManager)
        {
            this.flightsManager = flightsManager;
        }

        // GET: api/FlightPlan/5
        [HttpGet("{id}", Name = "Get")]
        public FlightPlan GetFlightPlanById(string id)
        {
            if ((id.Last() == '&'))
            {
                id = id.Remove(id.Length - 1);
                return flightsManager.GetFlightPlanById(id, true);
            }
            else
            {
                return flightsManager.GetFlightPlanById(id, false);
            }
        }

        // POST: api/FlightPlan
        [HttpPost]
        public void Post(FlightPlan fp)
        {
            flightsManager.addFlightPlan(fp);
        }
    }
}
