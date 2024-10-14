﻿using RestSharp;
using Spectre.Console;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;


namespace Stationboard
{
    class Program
    {
        public const string DEST_PLACE = "Zurich";
        public const int LIMIT = 3;

        static void Main(string[] args)
        {
            Console.WriteLine("Anzahl Argumente: {0}", args.Length);

            string station = DEST_PLACE;
            int limit = LIMIT;

            if (args.Length > 0)
            {
                foreach (var arg in args)
                {
                    // check if any of the parsed arguments is of data type int
                    if (isNumber(arg))
                    {
                        limit = Convert.ToInt32(arg);
                    }

                    // the parsed arguments is of data type string
                    else
                    {
                        station = arg;
                    }
                }                
            }

            static bool isNumber(string arg)
            {
                return int.TryParse(arg, out int value);
            }

            Console.WriteLine("Station: {0}, Display {1} departues\r\n", station, limit);
            Console.WriteLine("### Abfahrtstafel ###");
            
            StationboardResponse response = GetStationboard(station, limit);

            string headingFormat = String.Format("{0, -20} {1, -22} {2, -22} {3}", "Departure Time", "Delay in Minutes", "Vehicle", "Destination");
            Console.WriteLine(headingFormat);

            // iterates over every object in stationboard
            foreach (Stationboard sb in response.stationboard)
            {
                string departure = ExtractDepartureTime(sb);
                string delay = ExtractDelay(sb.stop.delay);
                string vehicle = ExtractVehicle(sb);

                string strFormat = String.Format("{0, -20} {1, -22} {2, -22} {3}", departure, delay, vehicle, sb.to);
                Console.WriteLine(strFormat);
            }
        }

        public static StationboardResponse GetStationboard(string getStation, int getLimit)
        {
            var client = CreateClient();
            var requestStationboard = CreateRequestStationboard(getStation, getLimit);
            var response = client.Get<StationboardResponse>(requestStationboard); // get json data
            return response;
        }

        public static RestClient CreateClient()
        {
            // TODO Max timeout?
            var options = new RestClientOptions("https://transport.opendata.ch/v1");
            return new RestClient(options);
        }

        public static RestRequest CreateRequestStationboard(string requestStation, int requestLimit)
        {
            RestRequest requestStationboard = new RestRequest("stationboard");
            requestStationboard.AddParameter("station", requestStation);
            requestStationboard.AddParameter("limit", requestLimit);
            requestStationboard.AddParameter("fields[]", "stationboard/stop/departure");
            requestStationboard.AddParameter("fields[]", "stationboard/stop/delay");
            requestStationboard.AddParameter("fields[]", "stationboard/category");
            requestStationboard.AddParameter("fields[]", "stationboard/number");
            requestStationboard.AddParameter("fields[]", "stationboard/to");
            return requestStationboard;
        }

        public static string ExtractDepartureTime(Stationboard sb) => DateTime.Parse(sb.stop.departure).ToString("HH:mm");

        public static string ExtractVehicle(Stationboard sb) => sb.category + sb.number;

        // TODO Stimmt diese Methode?
        public static string ExtractDelay(int? delay) => delay == null || delay == 0 ? "" : "+" + delay;

        public record Stationboard(StationboardStop stop, string to, string number, string category); // stop => nested inside stationboard
        public record StationboardStop(string departure, int? delay); // delay nullable value type
        public record StationboardResponse(Stationboard[] stationboard);
    }
}