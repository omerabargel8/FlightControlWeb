using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightsManager : IFlightsManager
    {
        private static ConcurrentDictionary<string, FlightPlan> flightPlans = new ConcurrentDictionary<string, FlightPlan>();
        private static ConcurrentDictionary<string, Flight> flights = new ConcurrentDictionary<string, Flight>();
        private static ConcurrentDictionary<string, Server> servers = new ConcurrentDictionary<string, Server>(); 
        private static ConcurrentDictionary<string, string> externalFlights = new ConcurrentDictionary<string, string>();
        private static Random random = new Random();
       
        //adding new flight plan
        public void addFlightPlan(FlightPlan fp)
        {
            //create random id
            string id = RandomString();
            flightPlans[id] = fp;
            flights[id] = new Flight { Flight_id = id, Latitude = fp.Initial_location.Latitude, Longitude = fp.Initial_location.Longitude, Passengers = fp.Passengers, Date_time = fp.Initial_location.Date_time, Is_extetanl = fp.IsExtetanl, Company_name = fp.Company_name };
        }
        //delete flight from dictionarys
        public void deleteFlight(string id) 
        {
            if (flights.ContainsKey(id)) {
                Flight f = flights[id];
                flights.TryRemove(id, out f);
                FlightPlan fp = flightPlans[id];
                flightPlans.TryRemove(id, out fp);
            }
        }
        //find and returns list of all relevent flights(internal/external)
        public List<Flight> getAllFlights(string relative_to, bool isExternals) 
        {
            List<Flight> flightsInTime = new List<Flight>();
            if (isExternals)
                //gets external flights
                flightsInTime = getFlightFromServers(relative_to);
            DateTime relativeTime = DateTime.Parse(relative_to).ToUniversalTime();
            foreach (var flight in flightPlans)
            {
                DateTime initial = flight.Value.Initial_location.Date_time;
                DateTime endtime = flight.Value.Initial_location.Date_time;
                foreach (var segment in flight.Value.Segments)
                {
                    endtime = endtime.AddSeconds(segment.Timespan_Seconds);
                }
                //checking if the flights is relevent
                if (endtime > relativeTime && initial < relativeTime)
                {
                    updatePosition(flight.Key, relativeTime);
                    flightsInTime.Add(flights[flight.Key]);
                }
            }
            return flightsInTime;
        }
        //get internal/external flight plan
        public FlightPlan GetFlightPlanById(string id, bool internRequest)
        {
            if (internRequest)
            {
                if (flightPlans.ContainsKey(id))
                    return flightPlans[id];
                else
                    return getExternalFlightPlan(id);
            }
            else
            {
                if (flightPlans.ContainsKey(id))
                    return flightPlans[id];
                else return null;
            }
        }
        //gets external flight plan from server
        public FlightPlan getExternalFlightPlan(string id)
        {
            if (externalFlights.ContainsKey(id))
            {
                return sendFlightPlanRequest(id);
            }
            else return null;
        }
        //send the server request for flight plan
        public FlightPlan sendFlightPlanRequest(string id)
        {
            string path, responseFromServer;
            path = externalFlights[id] + "/api/FlightPlan/" + id;
            WebRequest request = WebRequest.Create(path);
            try
            {
                WebResponse response = request.GetResponse();
                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                FlightPlan fp = JsonConvert.DeserializeObject<FlightPlan>(responseFromServer);
                return fp;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not receive data from {0}", externalFlights[id]);
                return null;
            }
        }
        //create random string 3 letters and 3 numbers
        public static string RandomString()
        {
            string letters, numbers;
            const string lettersChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbersChars = "0123456789";
            letters = new string(Enumerable.Repeat(lettersChars, 3)
            .Select(s => s[random.Next(s.Length)]).ToArray());
            numbers = new string(Enumerable.Repeat(numbersChars, 3)
            .Select(s => s[random.Next(s.Length)]).ToArray());
            return letters + numbers;
        }
        //returns list of all external servers
        public List<Server> getAllServers()
        {
            List<Server> serverList = new List<Server>();
            foreach (var server in servers)
            {
                serverList.Add(server.Value);
            }
            return serverList;
        }
        public void addServer(Server s)
        {
            servers[s.ServerId] = s;  
        }
        public void deleteServer(string id)
        {
            Server s = servers[id];
            servers.TryRemove(id, out s);
        }
        //find list of all relevant flights from all external servers
        public List<Flight> getFlightFromServers(string relative_to)
        {
            List<Flight> flights = new List<Flight>();
            foreach (var server in servers)
            {
                flights.AddRange(flightListRequest(server.Value.ServerURL, relative_to));
            }
            return flights;
        }
        //gets list of flights from server
        public List<Flight> flightListRequest(string url, string relative_to)
        {
            string path, responseFromServer;
            List<Flight> tempFlights = new List<Flight>();
            List<Flight> flights = new List<Flight>();
            path = url + "/api/Flights?relative_to=" + relative_to;
            WebRequest request = WebRequest.Create(path);
            try
            {
                WebResponse response = request.GetResponse();
                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    responseFromServer = reader.ReadToEnd();
                }
                tempFlights = JsonConvert.DeserializeObject<List<Flight>>(responseFromServer);
                foreach (var flight in tempFlights)
                {
                    flight.Is_extetanl = true;
                    //flights[flight.Flight_id] = flight;
                    bool b = externalFlights.TryAdd(flight.Flight_id, url);
                }
                return tempFlights;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not receive data from {0}", url);
                return null;
            }
        }
        //updating flight position
        public void updatePosition(string id, DateTime relativeTime)
        {
            double fraction;
            double newLat = flights[id].Latitude;
            double newLng = flights[id].Longitude;
            Segment prevSegment = new Segment { Latitude = flightPlans[id].Initial_location.Latitude, Longitude = flightPlans[id].Initial_location.Longitude, Timespan_Seconds = 0 };
            DateTime startPoint = flightPlans[id].Initial_location.Date_time;
            DateTime endPoint;
            foreach (var segment in flightPlans[id].Segments)
            {
                endPoint = startPoint.AddSeconds(segment.Timespan_Seconds);
                if (endPoint > relativeTime && startPoint < relativeTime)
                {
                    System.TimeSpan secondsPassed = relativeTime.Subtract(startPoint);
                    fraction = secondsPassed.TotalSeconds / segment.Timespan_Seconds;
                    newLat = fraction * (segment.Latitude - prevSegment.Latitude) + prevSegment.Latitude;
                    newLng = fraction * (segment.Longitude - prevSegment.Longitude) + prevSegment.Longitude;
                    break;
                }
                else {
                    prevSegment = segment;
                    startPoint = endPoint;
                     }
            }
            flights[id].Latitude = newLat;
            flights[id].Longitude = newLng;
        }
    }
}
