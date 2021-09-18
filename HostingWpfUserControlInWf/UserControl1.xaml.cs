using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using System.Windows.Media.Media3D;
using System.Windows.Forms.Integration;
using System.Windows.Media.Animation;

namespace HostingWpfUserControlInWf
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public double rotationY = Math.PI;
        public double rotationX = 0;
        public double elevation = 0; //Camera angel of elevation above rotation point
        public double camDist = 5; //Distance from rotation point
        public Point3D camRotationPoint = new Point3D(0, 0, 0);
        public bool ShiftPressed;

        public UserControl1()
        {
            InitializeComponent();
        }

        public void TranslateCameraXY(double x, double y) //Move the camera AND the rotation point
        {
            //Transform our initial x/y motion around the Y axis so it is applied in the camera plane
            double xp = (-1) * x * Math.Cos(rotationY); //+ 0 * Math.Sin(a); //z translation is zero
            double yp = y;
            double zp = (-1) * -x * Math.Sin(rotationY); //+ 0 * Math.Cos(a);

            Cam.Position = new Point3D() { X = Cam.Position.X + xp, Y = Cam.Position.Y + yp, Z = Cam.Position.Z + zp };
            camRotationPoint = new Point3D { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp };
        }

        public void RotateCameraY(double radians) //Rotate around rotation point, maintain distance
        {
            //Clamp rotation between +/- 2PI otherwise rotating long enough in one direction will go out of bounds!
            rotationY = (rotationY + radians) % (2 * Math.PI);
            //Find camera's position offset from the rotation point in the X/Z plane
            Vector3D os = new Vector3D(Cam.Position.X - camRotationPoint.X, 0, Cam.Position.Z - camRotationPoint.Z);

            double xp = os.Length * Math.Sin(rotationY);
            double yp = 0;  //We don't want any movement in the Y axis
            double zp = os.Length * Math.Cos(rotationY);

            Cam.Position = new Point3D() { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp, };
            Cam.LookDirection = new Vector3D(-xp, -yp, -zp);
        }

        public void RotateCameraXY(double rx, double ry)
        {
            //Clamp rotation values - we're allowing full rotation around Y, but only +/- 90 around X
            //rotationY = (rotationY + ry) % (2 * Math.PI);
            //rotationX = (rotationX + rx) % (2 * Math.PI);
            rotationX = Math.Min(rotationX + rx, 2 * Math.PI - 0.000001);
            rotationX = Math.Min(rotationX, 0.000001);
            rotationY = Math.Min(rotationY + ry, (Math.PI - 0.000001));
            rotationY = Math.Max(rotationY, 0.000001);

            double r = new Vector3D(Cam.Position.X - camRotationPoint.X, Cam.Position.Y - camRotationPoint.Y, Cam.Position.Z - camRotationPoint.Z).Length;

            double x = r * Math.Sin(rotationY) * Math.Cos(rotationX);
            double y = r * Math.Sin(rotationY) * Math.Sin(rotationX);
            double z = r * Math.Cos(rotationY);

            //double x = r * Math.Sin(Math.PI/2) * Math.Cos(Math.PI/4);
            //double y = r * Math.Sin(Math.PI/2) * Math.Sin(Math.PI/4);
            //double z = r * Math.Cos(Math.PI/2);

            Cam.Position = new Point3D() { X = camRotationPoint.X + x, Y = camRotationPoint.Y + y, Z = camRotationPoint.Z + z, };
            Cam.LookDirection = new Vector3D(-x, -y, -z);
        }

        //public void RotateCameraXY(double rx, double ry)
        //{
        //    Vector3D osy = new Vector3D(Cam.Position.X - camRotationPoint.X, Cam.Position.Y - camRotationPoint.Y, Cam.Position.Z - camRotationPoint.Z);

        //    double xp = osy.X * Math.Cos(ry) + osy.Z * Math.Sin(ry);
        //    double yp = osy.Y;
        //    double zp = -osy.X * Math.Sin(ry) + osy.Z * Math.Cos(ry);

        //    //Cam.Position = new Point3D() { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp };
        //    //Cam.LookDirection = new Vector3D(-xp, -yp, -zp);

        //    Vector3D osx = new Vector3D(xp, yp, zp);

        //    double xpp = xp;
        //    double ypp = osx.Y * Math.Cos(rx) - osx.Z * Math.Sin(rx);
        //    double zpp = osx.Y * Math.Sin(rx) + osx.Z * Math.Cos(rx);

        //    Cam.Position = new Point3D() { X = camRotationPoint.X + xpp, Y = camRotationPoint.Y + ypp, Z = camRotationPoint.Z + zpp };
        //    Cam.LookDirection = new Vector3D(-xpp, -ypp, -zpp);
        //}

        public void Zoom(double dist) //Move the camera while maintaining the rotation point
        {
            //Find camera's position offset from the rotation point in the X/Z plane
            Vector3D os = new Vector3D(Cam.Position.X - camRotationPoint.X, 0, Cam.Position.Z - camRotationPoint.Z);

            //os.Length + dist = new circle radius
            double xp = (os.Length + dist) * Math.Sin(rotationY);
            double yp = 0;  //We don't want any movement in the Y plane
            double zp = (os.Length + dist) * Math.Cos(rotationY);

            //If our new radius is too small, reject the camera movement to stop the camera freaking out with negative radius
            if (new Vector3D(xp, yp, zp).Length <= 0.25) return;

            Cam.Position = new Point3D() { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp, };
            Cam.LookDirection = new Vector3D(-xp, -yp, -zp);
        }

        public void ResetCamera()
        {
            rotationY = Math.PI;
            elevation = 0;
            Cam.Position = new Point3D(0, 0, -5);
            Cam.LookDirection = new Vector3D() { X = -Cam.Position.X, Y = -camRotationPoint.Y, Z = -Cam.Position.Z };
        }

        public void AddModel(GeometryModel3D model)
        {
            Group.Children.Add(model);
            CentreCamera();
            RotateCameraY(0);
        }

        public void ClearModels()
        {
            ResetCamera();
            for (int i = 0; i < Group.Children.Count; i++)
            {
                if (Group.Children[i].GetType() == typeof(GeometryModel3D))
                {
                    Group.Children.RemoveAt(i);
                    i--;
                }
            }
        }

        public void CentreCamera()
        {
            double minX = 0;
            double maxX = 0;

            double minY = 0;
            double maxY = 0;

            double minZ = 0;
            double maxZ = 0;

            List<double> xcentre = new List<double>();
            List<double> ycentre = new List<double>();
            List<double> zcentre = new List<double>();

            for (int i = 0; i < Group.Children.Count; i++)
            {
                if (Group.Children[i].GetType() == typeof(GeometryModel3D))
                {
                    var Model = (GeometryModel3D)Group.Children[i];

                    minX = Math.Min(Model.Bounds.Location.X, minX);
                    maxX = Math.Max(Model.Bounds.Location.X + Model.Bounds.SizeX, maxX);

                    minY = Math.Min(Model.Bounds.Location.Y, minY);
                    maxY = Math.Max(Model.Bounds.Location.Y + Model.Bounds.SizeY, maxY);

                    minZ = Math.Min(Model.Bounds.Location.Z, minZ);
                    maxZ = Math.Max(Model.Bounds.Location.Z + Model.Bounds.SizeZ, maxZ);

                    xcentre.Add(Model.Bounds.X + Model.Bounds.SizeX / 2);
                    ycentre.Add(Model.Bounds.Y + Model.Bounds.SizeY / 2);
                    zcentre.Add(Model.Bounds.Z + Model.Bounds.SizeZ / 2);
                }
            }

            if (xcentre.Count > 0 && ycentre.Count > 0 && zcentre.Count > 0)
            {
                rotationY = Math.PI;
                camRotationPoint = new Point3D(xcentre.Average(), ycentre.Average(), zcentre.Average());
                Cam.Position = new Point3D(camRotationPoint.X, camRotationPoint.Y, -camRotationPoint.Z + 4 + Math.Max(maxX - minX, maxZ - minZ));
                Cam.LookDirection = new Vector3D(-camRotationPoint.X, -camRotationPoint.Y, -Cam.Position.Z);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed || e.LeftButton == MouseButtonState.Pressed)
            {
                Point ScreenCentre = myViewport.PointToScreen(new System.Windows.Point((int)(myViewport.ActualWidth / 2), (int)(myViewport.ActualHeight / 2)));
                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)ScreenCentre.X, (int)ScreenCentre.Y);
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                double XCenter = Math.Round(myViewport.ActualWidth / 2);
                double YCenter = Math.Round(myViewport.ActualHeight / 2);

                double MouseX = e.GetPosition(myViewport).X;
                double MouseY = e.GetPosition(myViewport).Y;

                TranslateCameraXY(Math.Abs(MouseX - XCenter) * (MouseX - XCenter) / 300, Math.Abs(MouseY - YCenter) * (MouseY - YCenter) / 300);

                Point ScreenCentre = myViewport.PointToScreen(new Point((int)XCenter, (int)YCenter));

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)ScreenCentre.X, (int)ScreenCentre.Y);
            }

            else if (e.RightButton == MouseButtonState.Pressed && ShiftPressed == false)
            {
                double XCenter = Math.Round(myViewport.ActualWidth / 2);
                double YCenter = Math.Round(myViewport.ActualHeight / 2);

                double MouseX = e.GetPosition(myViewport).X;
                double MouseY = e.GetPosition(myViewport).Y;

                double radians = Math.Abs(MouseX - XCenter) * (MouseX - XCenter) / 150 * Math.PI / 180;

                Point ScreenCentre = myViewport.PointToScreen(new Point((int)XCenter, (int)YCenter));

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)ScreenCentre.X, (int)ScreenCentre.Y);

                RotateCameraY(radians);
            }

            else if (e.RightButton == MouseButtonState.Pressed && ShiftPressed == true)
            {

                double XCenter = Math.Round(myViewport.ActualWidth / 2);
                double YCenter = Math.Round(myViewport.ActualHeight / 2);

                double MouseY = e.GetPosition(myViewport).Y;

                double zoom = Math.Abs(MouseY - YCenter) * (MouseY - YCenter) / 250;

                Point ScreenCentre = myViewport.PointToScreen(new Point((int)XCenter, (int)YCenter));

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)ScreenCentre.X, (int)ScreenCentre.Y);

                Zoom(zoom);
            }
        }
    }
}
