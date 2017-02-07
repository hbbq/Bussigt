using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace Bussigt.App.Extensions
{

    internal static class LocationExtensions
    {

        private static double WGS84_RADIUS = 6370997.0;
        private static double EarthCircumFence = 2 * WGS84_RADIUS * Math.PI;

        private static Location getPosition(Location sourcePosition, double mEastWest, double mNorthSouth)
        {
            double degreesPerMeterForLat = EarthCircumFence / 360.0;
            double shrinkFactor = Math.Cos((sourcePosition.Latitude * Math.PI / 180));
            double degreesPerMeterForLon = degreesPerMeterForLat * shrinkFactor;
            double newLat = sourcePosition.Latitude + mNorthSouth * (1 / degreesPerMeterForLat);
            double newLng = sourcePosition.Longitude + mEastWest * (1 / degreesPerMeterForLon);
            var l = new Location(sourcePosition);
            l.Latitude = newLat;
            l.Longitude = newLng;
            return l;
        }

        public static Location GoWest(this Location @this, double meters)
        {
            return (getPosition(@this, -meters, 0));
        }

        public static Location GoEast(this Location @this, double meters)
        {
            return (getPosition(@this, meters, 0));
        }

        public static Location GoNorth(this Location @this, double meters)
        {
            return (getPosition(@this, 0, meters));
        }


        public static Location GoSouth(this Location @this, double meters)
        {
            return (getPosition(@this, 0, -meters));
        }

    }

}