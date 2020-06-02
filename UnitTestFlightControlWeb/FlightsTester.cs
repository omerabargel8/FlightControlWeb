using FlightControlWeb.Controllers;
using FlightControlWeb.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTestFlightControlWeb
{
    [TestClass]
    public class FlightsTester
    {
        [TestMethod]
        public void getFlightsTest()
        {
            FlightsManager mockFlightManager = new FlightsManager();
            //create first mock flightPlan start at 7:00 ends at 19:00
            List<Segment> segments1 = new List<Segment>();
            segments1.Add(new Segment { Longitude = 50, Latitude = 50, Timespan_Seconds = 3600*10 });
            segments1.Add(new Segment { Longitude = 70, Latitude = 70, Timespan_Seconds = 3600 });
            segments1.Add(new Segment { Longitude = 80, Latitude = 80, Timespan_Seconds = 3600 });
            FlightPlan mock1 = new FlightPlan
            {
                Segments = segments1,
                Passengers = 120,
                Company_name = "TestFlight1",
                Initial_location = new InitialLocation { Longitude = 40, Latitude = 40, Date_time = new DateTime(1000, 02, 01, 7, 0, 0) }
            };
            mockFlightManager.addFlightPlan(mock1);

            //create second mock flightPlan start at 7:00 ends at 10:00
            List<Segment> segments2 = new List<Segment>();
            segments2.Add(new Segment { Longitude = 50, Latitude = 50, Timespan_Seconds = 3600 });
            segments2.Add(new Segment { Longitude = 70, Latitude = 70, Timespan_Seconds = 3600 });
            segments2.Add(new Segment { Longitude = 80, Latitude = 80, Timespan_Seconds = 3600 });
            FlightPlan mock2 = new FlightPlan
            {
                Segments = segments2,
                Passengers = 120,
                Company_name = "TestFlight2",
                Initial_location = new InitialLocation { Longitude = 40, Latitude = 40, Date_time = new DateTime(1000, 02, 01, 7, 0, 0) }
            };
            mockFlightManager.addFlightPlan(mock2);

            //create third mock flightPlan start at 18:00 ends at 23:00
            List<Segment> segments3 = new List<Segment>();
            segments3.Add(new Segment { Longitude = 50, Latitude = 50, Timespan_Seconds = 3600 * 3 });
            segments3.Add(new Segment { Longitude = 70, Latitude = 70, Timespan_Seconds = 3600 });
            segments3.Add(new Segment { Longitude = 80, Latitude = 80, Timespan_Seconds = 3600 });
            FlightPlan mock3 = new FlightPlan
            {
                Segments = segments3,
                Passengers = 120,
                Company_name = "TestFlight3",
                Initial_location = new InitialLocation { Longitude = 40, Latitude = 40, Date_time = new DateTime(1000, 02, 01, 18, 0, 0) }
            };
            mockFlightManager.addFlightPlan(mock3);

            //create fourth mock flightPlan start at 12:00 ends at 21:00
            List<Segment> segments4 = new List<Segment>();
            segments4.Add(new Segment { Longitude = 50, Latitude = 50, Timespan_Seconds = 3600 });
            segments4.Add(new Segment { Longitude = 70, Latitude = 70, Timespan_Seconds = 3600 * 7 });
            segments4.Add(new Segment { Longitude = 80, Latitude = 80, Timespan_Seconds = 3600 });
            FlightPlan mock4 = new FlightPlan
            {
                Segments = segments4,
                Passengers = 120,
                Company_name = "TestFlight4",
                Initial_location = new InitialLocation { Longitude = 40, Latitude = 40, Date_time = new DateTime(1000, 02, 01, 12, 0, 0) }
            };
            mockFlightManager.addFlightPlan(mock4);
            //gets from the manager all related flights, should receive mock1 and mock4  
            List<Flight> flights = mockFlightManager.getAllFlights("1000-02-01T17:00:00Z", false).ToList();
            //returns true only if the manager returns mock1 and mock4 flights
            if (flights.Count() == 2)
            {
                bool option1 = (flights[0].Company_name == "TestFlight1" && flights[1].Company_name == "TestFlight4");
                bool option2 = (flights[0].Company_name == "TestFlight4" && flights[1].Company_name == "TestFlight1");
                Assert.IsTrue(option1 || option2);
            } else
                Assert.IsTrue(false);
        }
    }
}
