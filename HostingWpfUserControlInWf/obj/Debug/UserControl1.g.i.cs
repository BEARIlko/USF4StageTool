﻿#pragma checksum "..\..\UserControl1.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "21BB9F1E56CD166BF97C5AC04961FC51A8B45D60EF32BDDE6D1A29BDA570FE3F"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace HostingWpfUserControlInWf {
    
    
    /// <summary>
    /// UserControl1
    /// </summary>
    public partial class UserControl1 : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 17 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewport3D myViewport;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.PerspectiveCamera Cam;
        
        #line default
        #line hidden
        
        
        #line 36 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.Model3DGroup Group;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.GeometryModel3D Geom;
        
        #line default
        #line hidden
        
        
        #line 48 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.MeshGeometry3D Mesh;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.DiffuseMaterial Br;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\UserControl1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.DiffuseMaterial Br2;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/HostingWpfUserControlInWf;component/usercontrol1.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\UserControl1.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.myViewport = ((System.Windows.Controls.Viewport3D)(target));
            
            #line 17 "..\..\UserControl1.xaml"
            this.myViewport.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.myViewport_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 17 "..\..\UserControl1.xaml"
            this.myViewport.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.myViewport_MouseRightButtonDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Cam = ((System.Windows.Media.Media3D.PerspectiveCamera)(target));
            return;
            case 3:
            this.Group = ((System.Windows.Media.Media3D.Model3DGroup)(target));
            return;
            case 4:
            this.Geom = ((System.Windows.Media.Media3D.GeometryModel3D)(target));
            return;
            case 5:
            this.Mesh = ((System.Windows.Media.Media3D.MeshGeometry3D)(target));
            return;
            case 6:
            this.Br = ((System.Windows.Media.Media3D.DiffuseMaterial)(target));
            return;
            case 7:
            this.Br2 = ((System.Windows.Media.Media3D.DiffuseMaterial)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
