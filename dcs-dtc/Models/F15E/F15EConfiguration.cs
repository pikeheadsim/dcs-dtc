﻿using DTC.Utilities;
using DTC.Models.F15E.Displays;
using DTC.Models.F15E.Misc;
using DTC.Models.F15E.Radios;
using DTC.Models.F15E.Waypoints;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Globalization;

namespace DTC.Models.F15E
{
    public class F15EConfiguration : IConfiguration
    {
        public WaypointSystem Waypoints = new WaypointSystem();
        public RadioSystem Radios = new RadioSystem();
        public DisplaySystem Displays = new DisplaySystem();
        public MiscSystem Misc = new MiscSystem();

        public string ToJson()
        {
            var json = JsonConvert.SerializeObject(this);
            return json;
        }

        public string ToCompressedString()
        {
            var json = ToJson();
            return StringCompressor.CompressString(json);
        }

        public static F15EConfiguration FromJson(string s)
        {
            try
            {
                var cfg = JsonConvert.DeserializeObject<F15EConfiguration>(s);
                cfg.AfterLoadFromJson();
                return cfg;
            }
            catch
            {
                return null;
            }
        }

        public void AfterLoadFromJson()
        {
            if (this.Displays != null)
            {
                if (this.Displays.WSO == null) this.Displays.WSO = new WSODisplays();

                if (this.Displays.WSO.LeftMPCD == null) this.Displays.WSO.LeftMPCD = new DisplayConfig();
                if (this.Displays.WSO.LeftMPD == null) this.Displays.WSO.LeftMPD = new DisplayConfig();
                if (this.Displays.WSO.RightMPD == null) this.Displays.WSO.RightMPD = new DisplayConfig();
                if (this.Displays.WSO.RightMPCD == null) this.Displays.WSO.RightMPCD = new DisplayConfig();
            }
        }

        public static F15EConfiguration FromCompressedString(string s)
        {
            try
            {
                var json = StringCompressor.DecompressString(s);
                var cfg = FromJson(json);
                return cfg;
            }
            catch
            {
                return null;
            }
        }

        public static F15EConfiguration FromCombatFlite(string file, string flightName)
        {
            var doc = XDocument.Parse(file);
            F15EConfiguration cfg = new F15EConfiguration
            {
                Waypoints =
                {
                    Waypoints = new List<Waypoint>()
                },
                Displays = null,
                Misc = null,
                Radios = null
            };

            var flight = doc.XPathSelectElement($"//Route[Name='{flightName}']");
            if (flight == null)
                return cfg;

            int counter = 0;
            foreach (var (xmlWaypoint, i) in flight.XPathSelectElements("./Waypoints/Waypoint").Select((xmlWaypoint, i) => (xmlWaypoint, i)))
            {
                var lat = double.Parse(xmlWaypoint.Element("Lat")?.Value, CultureInfo.InvariantCulture);
                var lon = double.Parse(xmlWaypoint.Element("Lon")?.Value, CultureInfo.InvariantCulture);
                var ele = double.Parse(xmlWaypoint.Element("Altitude")?.Value, CultureInfo.InvariantCulture);

                if (lat == null || lon == null)
                    continue;

                var coordinate = new CoordinateSharp.Coordinate(lat, lon);

                cfg.Waypoints.Waypoints.Add(new Waypoint(i)
                {
                    Name = xmlWaypoint.Element("Name")?.Value,
                    Latitude = $"{coordinate.Latitude.Position} {coordinate.Latitude.Degrees:00}°{coordinate.Latitude.DecimalMinute:00.000}’".Replace(',', '.'),
                    Longitude = $"{coordinate.Longitude.Position} {coordinate.Longitude.Degrees:000}°{coordinate.Longitude.DecimalMinute:00.000}’".Replace(',', '.'),
                    Elevation = Convert.ToInt32(ele),
                });
            }

            return cfg;
        }

        public F15EConfiguration Clone()
        {
            var json = ToJson();
            var cfg = FromJson(json);
            return cfg;
        }

        public void CopyConfiguration(F15EConfiguration cfg)
        {
            if (cfg.Waypoints != null)
            {
                Waypoints = cfg.Waypoints;
            }
            if (cfg.Radios != null)
            {
                Radios = cfg.Radios;
            }
            if (cfg.Displays != null)
            {
                Displays = cfg.Displays;
            }
            if (cfg.Misc != null)
            {
                Misc = cfg.Misc;
            }
        }

        IConfiguration IConfiguration.Clone()
        {
            return Clone();
        }
    }
}
