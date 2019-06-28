using System.Collections.Generic;

namespace RoteiroOtimizado
{
    public class Root
    {
        //public Response response { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public Waypoint[] waypoints { get; set; }
        public int distance { get; set; }
        public int time { get; set; }
        public string description { get; set; }
    }

    public class Response
    {
        public Route[] route { get; set; }
    }

    public class Route
    {
        public Waypoint[] waypoint { get; set; }
        public Leg[] leg { get; set; }
    }

    public class Waypoint
    {
        //public Position mappedPosition { get; set; }
        //public Position originalPosition { get; set; }
        public string id { get; set; }
        public decimal lat { get; set; }
        public decimal lng { get; set; }
        public int sequence { get; set; }
    }

    public class Position
    {
        public decimal latitude { get; set; }
        public decimal longitude { get; set; }
    }

    public class Leg
    {
        public Maneuver[] maneuver { get; set; }
    }

    public class Maneuver
    {
        public Position position { get; set; }
        public int travelTime { get; set; }
        public int length { get; set; }
    }
}
