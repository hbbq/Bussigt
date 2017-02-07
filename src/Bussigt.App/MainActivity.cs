using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;

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

            //var fl = new float[1];

            //Android.Locations.Location.DistanceBetween(
            //    map.Projection.VisibleRegion.FarLeft.Latitude,
            //    map.Projection.VisibleRegion.FarLeft.Longitude,
            //    map.Projection.VisibleRegion.FarRight.Latitude,
            //    map.Projection.VisibleRegion.FarRight.Longitude,
            //    fl);

            //sze = fl[0] / 5;

            //foreach (var o in overlays.Values)
            //{
            //    o.SetDimensions(sze, sze);
            //}

        }

        private void Map_MyLocationChange(object sender, GoogleMap.MyLocationChangeEventArgs e)
        {

            map.Clear();
            //overlays.Clear();

            //var over = new GroundOverlayOptions();
            //over.Position(new LatLng(e.Location.Latitude, e.Location.Longitude), sze, sze);
            //over.InvokeImage(GetCustomBitmapDescriptor("500 >"));
            ////over.InvokeImage(BitmapDescriptorFactory.FromResource(Resource.Drawable.Icon));
            //over.InvokeBearing(b);
            //over.InvokeZIndex(9999);
            //var x = map.AddGroundOverlay(over);

            //overlays.Add("500", x);

            //b = (b + 15) % 360;

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

