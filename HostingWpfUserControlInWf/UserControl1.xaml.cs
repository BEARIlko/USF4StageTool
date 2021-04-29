﻿using System;
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

        //Controls
        public bool WheelPressed;
        public double XOffset = 0;
        public double YOffset = 0;

        public UserControl1()
        {
            InitializeComponent();
        }

        public void UpdateCamera(double radians)
        {
            rotation += radians;

            double x = camDist * Math.Sin(rotation);
            double y = Math.Tan(elevation) * camDist;
            double z = camDist * Math.Cos(rotation);
            //Cam.Position = new Point3D() { X = x, Y = camRotationPoint.Y, Z = camRotationPoint.Z + z };
            Cam.Position = new Point3D() { X = x + XOffset, Y = y + YOffset, Z = camRotationPoint.Z + z };
            Cam.LookDirection = new Vector3D() { X = -x, Y = -camRotationPoint.Y, Z = -z };
        }

        public void Zoom(float dist)
        {
            camDist += dist;
            UpdateCamera(0);
        }

        public void ResetCamera()
        {
            rotation = Math.PI;
            XOffset = 0;
            YOffset = 0;
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
            camDist = 4 + Math.Max(maxX - minX, maxZ - minZ);
            elevation = Math.Tanh(ycentre.Average() / camDist);
        }

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
		{
            WheelPressed = true;
		}

		private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
		{
            if (WheelPressed)
            {
                WheelPressed = false;
            }
		}

		private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
		{
            if (e.Delta > 0)
            {
                Zoom(-2f);
            }
            else Zoom(2f);
		}

		private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
            UpdateCamera(Math.PI / 12);
		}

		private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
            UpdateCamera(-Math.PI / 12);
		}

		private void Grid_MouseMove(object sender, MouseEventArgs e)
		{
            if (WheelPressed)
            {
                double XCenter = myViewport.ActualWidth / 2;
                double YCenter = myViewport.ActualHeight / 2;

                double MouseX = e.GetPosition(myViewport).X;
                double MouseY = e.GetPosition(myViewport).Y;

                XOffset = (MouseX - XCenter)/100;
                YOffset = (MouseY - YCenter)/100;

                UpdateCamera(0);
            }
		}
	}
}
