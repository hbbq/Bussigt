using System;
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System.Collections.Generic;
using Android.Graphics;
using System.Threading;
using System.Threading.Tasks;
using Bussigt.App.Extensions;
using Android.Locations;
using Stranne.VasttrafikNET;

namespace Bussigt.App
{
    [Activity(Label = "Bussigt.App", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback
    {
        
        private GoogleMap map;
        private bool firstPos = true;
        float sze = 100;

        private string vtKey = "uoR8tQVixIzM3qWXEjX3TMqoCfAa";
        private string vtSecret = "d4gjeLlFUJhyjmoTMdEAm61SIK4a";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            ActionBar.Hide();
            
            var mapOptions = new GoogleMapOptions()
                .InvokeMapType(GoogleMap.MapTypeHybrid)
                .InvokeZoomControlsEnabled(true)
                .InvokeCompassEnabled(true);

            var _myMapFragment = MapFragment.NewInstance(mapOptions);
            FragmentTransaction tx = FragmentManager.BeginTransaction();
            tx.Add(Resource.Id.MapHolder, _myMapFragment);
            tx.Commit();

            _myMapFragment.GetMapAsync(this);

            StartTasks();

        }

        private CancellationTokenSource _cts;

        private void StartTasks()
        {

            _cts = new CancellationTokenSource();
            Task.Factory.StartNewTaskContinuously(
                GetPositions,                
                _cts.Token,
                TimeSpan.FromSeconds(1)
            );

        }

        private void StopTasks()
        {

            _cts.Cancel();

        }

        int b = 0;
        Location l = null;
        JourneyPlannerService vtService = null;

        double minLat = 0;
        double minLon = 0;
        double maxLat = 0;
        double maxLon = 0;

        private void GetPositions()
        {

            if (map == null) return;

            try
            {

                var ls = new Location(l);
                RunOnUiThread(() =>
                {
                    ls.Latitude = (map.Projection.VisibleRegion.FarLeft.Latitude + map.Projection.VisibleRegion.FarRight.Latitude) / 2;
                    ls.Longitude = (map.Projection.VisibleRegion.FarLeft.Longitude + map.Projection.VisibleRegion.FarRight.Longitude) / 2;
                });

                vtService = vtService ?? new JourneyPlannerService(vtKey, vtSecret, Guid.NewGuid().ToString());

                var l1 = ls.GoNorth(2000).GoWest(2000);
                var l2 = ls.GoSouth(2000).GoEast(2000);

                //var minLat = Math.Min(l1.Latitude, l2.Latitude);
                //var maxLat = Math.Max(l1.Latitude, l2.Latitude);
                //var minLon = Math.Min(l1.Longitude, l2.Longitude);
                //var maxLon = Math.Max(l1.Longitude, l2.Longitude);

                RunOnUiThread(() =>
                {
                    minLat = map.Projection.VisibleRegion.LatLngBounds.Southwest.Latitude;
                    minLon = map.Projection.VisibleRegion.LatLngBounds.Southwest.Longitude;
                    maxLat = map.Projection.VisibleRegion.LatLngBounds.Northeast.Latitude;
                    maxLon = map.Projection.VisibleRegion.LatLngBounds.Northeast.Longitude;
                });

                if (minLat == 0 && maxLat == 0) return;

                var result = vtService.GetLiveMap(minLon, maxLon, minLat, maxLat, true);

                RunOnUiThread(() => map.Clear());

                overlays.Clear();

                foreach (var vehicle in result.Vehicles)
                {

                    var over = new GroundOverlayOptions();
                    //over.Position(new LatLng(l.Latitude, l.Longitude), sze, sze);
                    over.Position(new LatLng(vehicle.Y / 1000000.0, vehicle.X / 1000000.0), sze, sze);
                    over.InvokeImage(GetCustomBitmapDescriptor(vehicle.Name.PadRight(4, ' ') + ">"));
                    //over.InvokeImage(BitmapDescriptorFactory.FromResource(Resource.Drawable.Icon));
                    over.InvokeBearing((float)((32.0 - (double)vehicle.Direction) * 360.0 / 32.0));
                    over.InvokeZIndex(9999);

                    RunOnUiThread(() =>
                    {
                        var x = map.AddGroundOverlay(over);
                        overlays.Add(vehicle.Gid.ToString(), x);
                    });

                }

                b = (b + 5) % 360;

                if (overlays.Count > 0)
                {
                    Thread.Sleep(20000);
                }
                else
                {
                    Thread.Sleep(2000);
                }

            }
            catch (Exception ex)
            {

                RunOnUiThread(() => map.Clear());

                vtService = null;

                System.Diagnostics.Debug.WriteLine(ex.Message);

                Thread.Sleep(TimeSpan.FromSeconds(2));

            }
            
        }

        public void OnMapReady(GoogleMap googleMap)
        {

            map = googleMap;
            map.MyLocationEnabled = true;
            map.MyLocationChange += Map_MyLocationChange;
            map.CameraChange += Map_CameraChange;

        }

        private void Map_CameraChange(object sender, GoogleMap.CameraChangeEventArgs e)
        {

            var fl = new float[1];

            Android.Locations.Location.DistanceBetween(
                map.Projection.VisibleRegion.FarLeft.Latitude,
                map.Projection.VisibleRegion.FarLeft.Longitude,
                map.Projection.VisibleRegion.FarRight.Latitude,
                map.Projection.VisibleRegion.FarRight.Longitude,
                fl);

            sze = fl[0] / 5;

            foreach (var o in overlays.Values)
            {
                o.SetDimensions(sze, sze);
            }
            
        }

        private BitmapDescriptor GetCustomBitmapDescriptor(string text)
        {
            using (Paint paint = new Paint(PaintFlags.AntiAlias))
            {

                paint.TextSize = 30;

                using (Rect bounds = new Rect())
                {
                    using (Bitmap baseBitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.Icon))
                    {
                        //Bitmap bitmap = baseBitmap.Copy(Bitmap.Config.Argb8888, true);

                        paint.SetTypeface(Typeface.Monospace);

                        paint.GetTextBounds(text, 0, text.Length, bounds);

                        var s = Math.Max((int)bounds.Width() + 12, (int)bounds.Height() + 12);
                        var bitmap = Bitmap.CreateBitmap(s, s, Bitmap.Config.Argb8888);

                        float x = (bitmap.Width - bounds.Width()) / 2.0f;
                        float y = (bitmap.Height - bounds.Height()) / 2.0f - bounds.Top;

                        Canvas canvas = new Canvas(bitmap);

                        paint.Color = Color.LightBlue;
                        paint.SetStyle(Paint.Style.FillAndStroke);
                        canvas.DrawRoundRect(x - 6, y + bounds.Top - 6, bounds.Width() + 6 + x, bounds.Height() + bounds.Top + y + 6, 10f, 10f, paint);
                        paint.Color = Color.Black;
                        canvas.DrawText(text, x, y, paint);

                        BitmapDescriptor icon = BitmapDescriptorFactory.FromBitmap(bitmap);

                        return (icon);
                    }
                }
            }
        }

        Dictionary<string, GroundOverlay> overlays = new Dictionary<string, GroundOverlay>();

        private void Map_MyLocationChange(object sender, GoogleMap.MyLocationChangeEventArgs e)
        {

            l = e.Location;

            if (firstPos)
            {
                map.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(new LatLng(e.Location.Latitude, e.Location.Longitude), 13));
                firstPos = false;

            }
            else
            {
                //map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(e.Location.Latitude, e.Location.Longitude)));
            };
        }

    }

}

