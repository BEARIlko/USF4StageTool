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

        public UserControl1()
        {
            InitializeComponent();
        }

        public void UpdateCamera(double radians)
        {
            rotation += radians;

            double x = camDist * Math.Sin(rotation);
            double z = camDist * Math.Cos(rotation);

            Cam.Position = new Point3D() { X = x, Y = Cam.Position.Y, Z = z };
            Cam.LookDirection = new Vector3D() { X = -Cam.Position.X, Y = -Cam.Position.Y, Z = -Cam.Position.Z };
        }

        public void ResetCamera()
        {
            rotation = Math.PI;

            Cam.Position = new Point3D() { X = 0, Y = 1, Z = -camDist };
            Cam.LookDirection = new Vector3D() { X = -Cam.Position.X, Y = -Cam.Position.Y, Z = -Cam.Position.Z };
        }

        public void AddModel(GeometryModel3D model)
        {
            Group.Children.Add(model);
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
    }
}
