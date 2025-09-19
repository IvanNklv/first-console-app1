
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AirportSimulation
{
    class Passenger
    {
        public string Name { get; set; }
        public string FlightNumber { get; set; }
        public bool HasTicket { get; set; }
        public bool PassedSecurity { get; set; }
        public bool IsOnBoard { get; set; }

        public Passenger(string name, string flightNumber)
        {
            Name = name;
            FlightNumber = flightNumber;
            HasTicket = false;
            PassedSecurity = false;
            IsOnBoard = false;
        }
    }

    class Flight
    {
        public string FlightNumber { get; set; }
        public string Destination { get; set; }
        public int DepartureTime { get; set; } 
        public string Status { get; set; }
        public int Capacity { get; set; }
        public List<Passenger> BoardedPassengers { get; set; }

        public Flight(string number, string destination, int departure, int capacity)
        {
            FlightNumber = number;
            Destination = destination;
            DepartureTime = departure;
            Capacity = capacity;
            Status = "OnTime";
            BoardedPassengers = new List<Passenger>();
        }
    }

    class Airport
    {
        private Random rnd = new Random();
        public List<Flight> Flights { get; set; } = new List<Flight>();
        public List<Passenger> Passengers { get; set; } = new List<Passenger>();
        public Queue<Passenger> CheckInQueue { get; set; } = new Queue<Passenger>();
        public Queue<Passenger> SecurityQueue { get; set; } = new Queue<Passenger>();
        public int Time { get; set; } = 0;

        private int checkInDesks = 3;
        private int securityPoints = 2;
        private int boardingSpeed = 5;

        public void Tick()
        {
            Time++;

         
            if (rnd.Next(0, 100) < 30) 
            {
                var flight = Flights[rnd.Next(Flights.Count)];
                var passenger = new Passenger("Passenger" + rnd.Next(1000, 9999), flight.FlightNumber);
                Passengers.Add(passenger);
                CheckInQueue.Enqueue(passenger);
                Console.WriteLine($"[NEW] {passenger.Name} прийшов на рейс {flight.FlightNumber}");
            }

            for (int i = 0; i < checkInDesks; i++)
            {
                if (CheckInQueue.Count > 0)
                {
                    var p = CheckInQueue.Dequeue();
                    p.HasTicket = true;
                    SecurityQueue.Enqueue(p);
                    Console.WriteLine($"[CHECK-IN] {p.Name} отримав квиток на {p.FlightNumber}");
                }
            }

   
            for (int i = 0; i < securityPoints; i++)
            {
                if (SecurityQueue.Count > 0)
                {
                    var p = SecurityQueue.Dequeue();
                    p.PassedSecurity = true;
                    Console.WriteLine($"[SECURITY] {p.Name} пройшов контроль.");
                }
            }

         
            foreach (var f in Flights.ToList())
            {
                if (Time == f.DepartureTime - 2 && f.Status == "OnTime")
                {
                    f.Status = "Boarding";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[BOARDING] Рейс {f.FlightNumber} ({f.Destination}) розпочав посадку!");
                    Console.ResetColor();
                }
                else if (Time == f.DepartureTime)
                {
                    f.Status = "Departed";
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[DEPARTED] Рейс {f.FlightNumber} ({f.Destination}) вилетів!");
                    Console.ResetColor();

                    foreach (var p in f.BoardedPassengers)
                        Passengers.Remove(p);
                    Flights.Remove(f);
                }
            }

         
            foreach (var f in Flights.Where(fl => fl.Status == "Boarding"))
            {
                var ready = Passengers
                    .Where(p => p.FlightNumber == f.FlightNumber && p.HasTicket && p.PassedSecurity && !p.IsOnBoard)
                    .Take(boardingSpeed)
                    .ToList();

                foreach (var p in ready)
                {
                    p.IsOnBoard = true;
                    f.BoardedPassengers.Add(p);
                    Console.WriteLine($"[BOARD] {p.Name} зайшов у літак {f.FlightNumber}");
                }
            }

            PrintStatus();
        }

        private void PrintStatus()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=== TIME: {Time} ===");
            Console.ResetColor();

            foreach (var f in Flights)
            {
                Console.WriteLine($"Flight {f.FlightNumber} to {f.Destination} - {f.Status}");
            }

            Console.WriteLine($"Черга на реєстрацію: {CheckInQueue.Count}");
            Console.WriteLine($"Черга на контроль: {SecurityQueue.Count}");
            Console.WriteLine($"Очікують у залі: {Passengers.Count(p => p.HasTicket && p.PassedSecurity && !p.IsOnBoard)}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Airport airport = new Airport();

          
            airport.Flights.Add(new Flight("PS101", "Kyiv", 5, 100));
            airport.Flights.Add(new Flight("PS202", "London", 8, 120));
            airport.Flights.Add(new Flight("PS303", "Berlin", 12, 80));

            while (true)
            {
                airport.Tick();
                Thread.Sleep(1000);
            }
        }
    }
}