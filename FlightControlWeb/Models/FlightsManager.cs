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

        public FlightsManager()
        {
            /**
            flights["DJ44JD"] = new Flight { Flight_id = "DJ44JD", Longitude = 40, Latitude = 40, Passengers = 3, Company_name = "omer airlines", Date_time = new DateTime(2020, 3, 1, 7, 0, 0), Is_extetanl = false };
            List<Segment> segments1 = new List<Segment>();
            segments1.Add(new Segment { Longitude = 50, Latitude = 50, Timespan_Seconds = 950 });
            segments1.Add(new Segment { Longitude = 70, Latitude = 70, Timespan_Seconds = 55000 });
            segments1.Add(new Segment { Longitude = 80, Latitude = 80, Timespan_Seconds = 55000 });
            flightPlans["DJ44JD"] = new FlightPlan
            {
                Segments = segments1,
                Passengers = 120,
                Company_name = "OrelFlightsLtd",
                Initial_location = new InitialLocation { Longitude = 40, Latitude = 40, Date_time = new DateTime(2020, 5, 31, 7, 0, 0) }
            };
    */
            Server s = new Server { ServerId = "123", ServerURL = "http://rony7.atwebpages.com" };
            Server s2 = new Server { ServerId = "124", ServerURL = "http://ronyut4.atwebpages.com" };

            servers[s.ServerId] = s;
            servers[s2.ServerId] = s2;

        }
       
        public void addFlightPlan(FlightPlan fp)
        {
            string id = RandomString(6);
            flightPlans[id] = fp;
            flights[id] = new Flight { Flight_id = id, Latitude = fp.Initial_location.Latitude, Longitude = fp.Initial_location.Longitude, Passengers = fp.Passengers, Date_time = fp.Initial_location.Date_time, Is_extetanl = fp.IsExtetanl, Company_name = fp.Company_name };
            Console.WriteLine("DDDDDD {0} XXXX {1}", flights.Count(), flightPlans.Count());
        }
        public void deleteFlight(string id) 
        {
            if (flights.ContainsKey(id)) {
                Flight f = flights[id];
                flights.TryRemove(id, out f);
                FlightPlan fp = flightPlans[id];
                flightPlans.TryRemove(id, out fp);
            }
        }
        public List<Flight> getAllFlights(string relative_to, bool isExternals) 
        {
            List<Flight> flightsInTime = new List<Flight>();
            if (isExternals)
                flightsInTime = getFlightFromServers(relative_to);
            DateTime relativeTime = DateTime.Parse(relative_to);
            foreach (var flight in flightPlans)
            {
                DateTime initial = flight.Value.Initial_location.Date_time;
                DateTime endtime = flight.Value.Initial_location.Date_time;
                foreach (var segment in flight.Value.Segments)
                {
                    endtime = endtime.AddSeconds(segment.Timespan_Seconds);
                }
                if (endtime > relativeTime && initial < relativeTime)
                {
                    updatePosition(flight.Key, relativeTime);
                    flightsInTime.Add(flights[flight.Key]);
                }
            }
    
            return flightsInTime;
        }

        public FlightPlan GetFlightPlanById(string id, bool internRequest)
        {
            string path, responseFromServer;
            if (internRequest)
            {
                if (flightPlans.ContainsKey(id))
                    return flightPlans[id];
                else
                {
                    if (externalFlights.ContainsKey(id))
                    {

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
                    else return null;
                }
            }
        else
            {
                if (flightPlans.ContainsKey(id))
                    return flightPlans[id];
                else return null;
            }
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        }
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
        public List<Flight> getFlightFromServers(string relative_to)
        {
            string path;
            string responseFromServer;
            List<Flight> tempFlights = new List<Flight>();
            List<Flight> flights = new List<Flight>();
            foreach (var server in servers)
            {
                path = server.Value.ServerURL + "/api/Flights?relative_to=" + relative_to;
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
                        bool b = externalFlights.TryAdd(flight.Flight_id, server.Value.ServerURL);
                    }
                    flights.AddRange(tempFlights);
                    tempFlights.Clear();
                } catch(Exception e)
                {
                    Console.WriteLine("Could not receive data from {0}", server.Value.ServerURL);
                }
            }
            return flights;
        }
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
                    Console.WriteLine("big {0} small {1}", segment.Timespan_Seconds, segment.Timespan_Seconds);
                    Console.WriteLine("sasa {0}", fraction);
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
