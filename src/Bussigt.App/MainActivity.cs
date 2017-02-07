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

namespace Bussigt.App
{
    [Activity(Label = "Bussigt.App", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IOnMapReadyCallback
    {
        
        private GoogleMap map;
        private bool firstPos = true;
        float sze = 100;

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
                TimeSpan.FromMilliseconds(500)
            );

        }

        private void StopTasks()
        {

            _cts.Cancel();

        }

        int b = 0;
        Location l = null;

        private void GetPositions()
        {

            if (l == null) return;

            RunOnUiThread(() => map.Clear());
                        
            overlays.Clear();

            var over = new GroundOverlayOptions();
            over.Position(new LatLng(l.Latitude, l.Longitude), sze, sze);
            over.InvokeImage(GetCustomBitmapDescriptor("500 >"));
            //over.InvokeImage(BitmapDescriptorFactory.FromResource(Resource.Drawable.Icon));
            over.InvokeBearing(b);
            over.InvokeZIndex(9999);

            RunOnUiThread(() => {
                var x = map.AddGroundOverlay(over);
                overlays.Add("500", x);
            });


            b = (b + 5) % 360;

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
                map.AnimateCamera(CameraUpdateFactory.NewLatLng(new LatLng(e.Location.Latitude, e.Location.Longitude)));
            };
        }

    }

}

