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
        public double camDist = 5;
        public Point3D camRotationPoint = new Point3D(0, 0, 0);

        public UserControl1()
        {
            InitializeComponent();
        }

        public void UpdateCamera(double radians)
        {
            rotation += radians;

            double x = camDist * Math.Sin(rotation);
            double z = camDist * Math.Cos(rotation);

            Cam.Position = new Point3D() { X = x, Y = camRotationPoint.Y, Z = camRotationPoint.Z + z };
            Cam.LookDirection = new Vector3D() { X = -x, Y = -camRotationPoint.Y, Z = -z };
        }

        public void ResetCamera()
        {
            rotation = Math.PI;

            Cam.Position = new Point3D() { X = 0, Y = camRotationPoint.Y, Z = camDist * Math.Cos(rotation) + camRotationPoint.Z };
            Cam.LookDirection = new Vector3D() { X = -Cam.Position.X, Y = -camRotationPoint.Y, Z = -Cam.Position.Z };
        }

        public void AddModel(GeometryModel3D model)
        {
            Group.Children.Add(model);
            CentreCamera();
            ResetCamera();
            UpdateCamera(0);
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

        private void myViewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateCamera(Math.PI / 12);
        }

        private void myViewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateCamera(-Math.PI / 12);
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

            camRotationPoint = new Point3D(xcentre.Average(), maxY + 1, zcentre.Average());
            camDist = 4 + Math.Max(maxX - minX, maxZ - minZ);
        }
    }
}
