﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using LinqToTwitter.Common;
using LitJson;

namespace LinqToTwitter
{
    /// <summary>
    /// Geographical area
    /// </summary>
    public class Geometry
    {
        public Geometry() {}
        internal Geometry(JsonData geometry)
        {
            if (geometry == null) return;

            Type = geometry.GetValue<string>("type");

            var coordinates = geometry.GetValue<JsonData>("coordinates");
            Coordinates =
                (from JsonData outer in coordinates
                 from JsonData coord in outer
                 select new Coordinate(coord))
                .ToList();
        }

        /// <summary>
        /// Converts XML to a new Geometry
        /// </summary>
        /// <param name="geometry">XML to convert</param>
        /// <returns>Geometry containing info from XML</returns>
        public Geometry CreateGeometry(XElement geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            List<Coordinate> coords = new List<Coordinate>();

            if (geometry.Element("coordinates") == null)
            {
                XNamespace geoRss = "http://www.georss.org/georss";

                if (geometry.Element(geoRss + "polygon") != null)
                {
                    var coordArr = geometry.Element(geoRss + "polygon").Value.Split(' ');

                    for (int lat = Coordinate.LatitudePos, lon = Coordinate.LongitudePos; lon < coordArr.Length; lat+=2, lon += 2)
                    {
                        coords.Add(
                            new Coordinate 
                            {
                                Latitude = double.Parse(coordArr[lat], CultureInfo.InvariantCulture),
                                Longitude = double.Parse(coordArr[lon], CultureInfo.InvariantCulture) 
                            });
                    }
                }
            }
            else
            {
                coords =
                    (from coord in geometry.Element("coordinates").Element("item").Elements("item")
                     select Coordinate.CreateCoordinate(coord))
                     .ToList();
            }

            return new Geometry
            {
                Type = 
                    geometry.Element("type") == null ?
                        string.Empty :
                        geometry.Element("type").Value,
                Coordinates = coords
            };
        }

        /// <summary>
        /// Type of bouding box
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Coordinates for bounding box
        /// </summary>
        public List<Coordinate> Coordinates { get; set; }
    }
}
