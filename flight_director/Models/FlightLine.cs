﻿using SQLite;
using System;


namespace flight_director.Models
{
    public class FlightLine
    {
        [PrimaryKey]
        public int ID { get; set; }
        public double StartLat { get; set; }
        public double StartLon { get; set; }
        public double EndLat { get; set; }
        public double EndLon { get; set; }

    }
}