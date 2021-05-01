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

namespace HostingWpfUserControlInWf
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        public double rotation = Math.PI;
        public double elevation = 0; //Camera angel of elevation above rotation point
        public double camDist = 5; //Distance from rotation point
        public Point3D camRotationPoint = new Point3D(0, 0, 0);

        public UserControl1()
        {
            InitializeComponent();
        }

        public void TranslateCameraXY(double x, double y) //Move the camera AND the rotation point
        {
            //Transform our initial x/y motion around the Y axis so it is applied in the camera plane
            double xp = (-1) * x * Math.Cos(rotation); //+ 0 * Math.Sin(a); //z translation is zero
            double yp = y;
            double zp = (-1) * -x * Math.Sin(rotation); //+ 0 * Math.Cos(a);

            Cam.Position = new Point3D() { X = Cam.Position.X + xp, Y = Cam.Position.Y + yp, Z = Cam.Position.Z + zp };
            camRotationPoint = new Point3D { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp };
        }

        public void RotateCameraY(double radians) //Rotate around rotation point, maintain distance
        {
            //Clamp rotation between +/- 2PI otherwise rotating long enough in one direction will go out of bounds!
            rotation = (rotation + radians) % (2 * Math.PI);
            //Find camera's position offset from the rotation point in the X/Z plane
            Vector3D os = new Vector3D(Cam.Position.X - camRotationPoint.X, 0, Cam.Position.Z - camRotationPoint.Z);

            double xp = os.Length * Math.Sin(rotation);
            double yp = 0;  //We don't want any movement in the Y plane
            double zp = os.Length * Math.Cos(rotation);

            Cam.Position = new Point3D() { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp, };
            Cam.LookDirection = new Vector3D(-xp, -yp, -zp);
        }

        public void Zoom(double dist) //Move the camera while maintaining the rotation point
        {
            //Find camera's position offset from the rotation point in the X/Z plane
            Vector3D os = new Vector3D(Cam.Position.X - camRotationPoint.X, 0, Cam.Position.Z - camRotationPoint.Z);

            //os.Length + dist = new circle radius
            double xp = (os.Length + dist) * Math.Sin(rotation);
            double yp = 0;  //We don't want any movement in the Y plane
            double zp = (os.Length + dist) * Math.Cos(rotation);

            Cam.Position = new Point3D() { X = camRotationPoint.X + xp, Y = camRotationPoint.Y + yp, Z = camRotationPoint.Z + zp, };
            Cam.LookDirection = new Vector3D(-xp, -yp, -zp);
        }

        public void ResetCamera()
        {
            rotation = Math.PI;
            elevation = 0;
            Cam.Position = new Point3D(0,0,-5);
            Cam.LookDirection = new Vector3D() { X = -Cam.Position.X, Y = -camRotationPoint.Y, Z = -Cam.Position.Z };
        }

        public void AddModel(GeometryModel3D model)
        {
            Group.Children.Add(model);
            CentreCamera();
            ResetCamera();
        }

        public void ClearModels()
        {
            ResetCamera();
            for (int i = 0; i < Group.Children.Count; i++)
            {
                if(Group.Children[i].GetType() == typeof(GeometryModel3D))
                {
                    Group.Children.RemoveAt(i);
                    i--;
                }
            }
        }

        private void CentreCamera()
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

            camRotationPoint = new Point3D(xcentre.Average(), ycentre.Average(), zcentre.Average());
            //Cam.Position = new Point3D(camRotationPoint.X, camRotationPoint.Y, camRotationPoint.Z + 4 + Math.Max(maxX - minX, maxZ - minZ));
            //Cam.LookDirection = new Vector3D(-camRotationPoint.X, -camRotationPoint.Y, -camRotationPoint.Z);
            camDist = 4 + Math.Max(maxX - minX, maxZ - minZ);
            //elevation = Math.Tanh(ycentre.Average() / camDist);
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

            else if (e.RightButton == MouseButtonState.Pressed)
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

            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                double XCenter = Math.Round(myViewport.ActualWidth / 2);
                double YCenter = Math.Round(myViewport.ActualHeight / 2);

                double MouseX = e.GetPosition(myViewport).X;
                double MouseY = e.GetPosition(myViewport).Y;

                double zoom = Math.Abs(MouseY - YCenter) * (MouseY - YCenter) / 250;

                Point ScreenCentre = myViewport.PointToScreen(new Point((int)XCenter, (int)YCenter));

                System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)ScreenCentre.X, (int)ScreenCentre.Y);

                Zoom(zoom);
            }
        }
	}
}
