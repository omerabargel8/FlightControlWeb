using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Models
{
    public class FlightsManager : IFlightsManager
    {
        private static ConcurrentDictionary<string, FlightPlan> flightPlans = new ConcurrentDictionary<string, FlightPlan>();
        private static ConcurrentDictionary<string, Flight> flights = new ConcurrentDictionary<string, Flight>();
        private static ConcurrentDictionary<string, Server> servers = new ConcurrentDictionary<string, Server>();
        private static Random random = new Random();

        public FlightsManager()
        {
            flights["69"] = new Flight { Flight_id = "69", Longitude = 0, Latitude = 0, Passengers = 3, Company_name = "omer airlines", Data_time = new DateTime(2020, 3, 1, 7, 0, 0), Is_extetanl = false };
            List<Segment> segments1 = new List<Segment>();
            segments1.Add(new Segment { Longitude = 70, Latitude = 70, Timespan_Seconds = 950 });
            segments1.Add(new Segment { Longitude = 75, Latitude = 75.34, Timespan_Seconds = 55000 });
            segments1.Add(new Segment { Longitude = 80, Latitude = 80, Timespan_Seconds = 100000 });
            flightPlans["69"] = new FlightPlan
            {
                Segments = segments1,
                Passengers = 120,
                Company_name = "OrelFlightsLtd",
                Initial_location = new InitialLocation { Longitude = 50, Latitude = 50, Date_time = new DateTime(2020, 5, 24, 7, 0, 0) }
            };

        }
            /**
                    private static List<Flight> Flights = new List<Flight>()
                    {
                        new Flight{ Flight_id=69, Longitude=0,  Latitude=0, Passengers=3, Company_name= "omer airlines", Data_time= new DateTime(2020, 3, 1, 7, 0, 0), Is_extetanl=false }
                    };



                    private static List<Segment> segments1 = new List<Segment>()
                        {
                            new Segment{Longitude=70,Latitude=70,Timespan_Seconds=950 },
                            new Segment{Longitude=75,Latitude=75.34,Timespan_Seconds=550 },
                            new Segment{Longitude=80,Latitude=80,Timespan_Seconds=1000 }
                    };
                    private static List<FlightPlan> flightPlans = new List<FlightPlan>()
                    {
                        new FlightPlan{ Segments = segments1, Passengers = 120, CompanyName = "OrelFlightsLtd",
                            InitialLocation = new InitialLocation { Longitude = 50, Latitude = 50, DateTime = new DateTime(2020, 3, 1, 7, 0, 0) } }
                    };
                */
       
        public void addFlightPlan(FlightPlan fp)
        {
            string id = RandomString(6);
            flightPlans[id] = fp;
            flights[id] = new Flight { Flight_id = id, Latitude = fp.Initial_location.Latitude, Longitude = fp.Initial_location.Longitude, Passengers = fp.Passengers, Data_time = fp.Initial_location.Date_time, Is_extetanl = fp.IsExtetanl, Company_name = fp.Company_name };
        }
        public void deleteFlight(string id) 
        {
            Flight f = flights[id];
            FlightPlan fp = flightPlans[id];
            flights.TryRemove(id, out f);
            flightPlans.TryRemove(id, out fp);
        }
        public List<Flight> getAllFlights(string relative_to, bool isExternals) 
        {
            List<Flight> flightsInTime = new List<Flight>();
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
                    if(isExternals)
                        flightsInTime.Add(flights[flight.Key]);

                    if(isExternals == false && flight.Value.IsExtetanl == false)
                        flightsInTime.Add(flights[flight.Key]);
                }
                Console.WriteLine("blaa");
            }
    
            return flightsInTime;
        }

        public FlightPlan GetFlightPlanById(string id)
        {
            return flightPlans[id];
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

    }
}
