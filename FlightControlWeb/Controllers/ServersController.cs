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
    public class ServersController : ControllerBase
    {
        private IFlightsManager flightsManager = new FlightsManager();
        public ServersController(IFlightsManager flightsManager)
        {
            this.flightsManager = flightsManager;
        }
        // GET: api/Servers
        [HttpGet]
        public IEnumerable<Server> GetAllServers()
        {
            return flightsManager.getAllServers();
        }

        // POST: api/Servers
        [HttpPost]
        public void Post(Server s)
        {
            flightsManager.addServer(s);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            flightsManager.deleteServer(id);
        }
    }
}
