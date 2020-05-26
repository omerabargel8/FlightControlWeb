using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public interface IFlightsManager
    {
        //void addFlight(Flight f);
        //void upadateFlight(Flight f);
        void deleteFlight(string id);
        List<Flight> getAllFlights(string relative_to, bool isExternals);
        List<Server> getAllServers();
        void addFlightPlan(FlightPlan fp);
        void addServer(Server s);
        FlightPlan GetFlightPlanById(string id, bool internRequest);
        void deleteServer(string id);
    }
}
