﻿#pragma checksum "..\..\..\DUT_Configuration.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3C3C4BF2218F6FA04BDD56B4E2ABE545CBBE0D30"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using QSC_Test_Automation;
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


namespace QSC_Test_Automation {
    
    
    /// <summary>
    /// DUT_Configuration
    /// </summary>
    public partial class DUT_Configuration : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 5 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal QSC_Test_Automation.DUT_Configuration DUT_Config;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grd;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid_ConfigFile;
        
        #line default
        #line hidden
        
        
        #line 286 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock External_devices_configuration;
        
        #line default
        #line hidden
        
        
        #line 287 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dataGrid_GeneratorConfigFile;
        
        #line default
        #line hidden
        
        
        #line 365 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtStatus;
        
        #line default
        #line hidden
        
        
        #line 366 "..\..\..\DUT_Configuration.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Save;
        
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
            System.Uri resourceLocater = new System.Uri("/QSC_Test_Automation;component/dut_configuration.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\DUT_Configuration.xaml"
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
            this.DUT_Config = ((QSC_Test_Automation.DUT_Configuration)(target));
            
            #line 5 "..\..\..\DUT_Configuration.xaml"
            this.DUT_Config.ContentRendered += new System.EventHandler(this.DUT_Config_ContentRendered);
            
            #line default
            #line hidden
            
            #line 5 "..\..\..\DUT_Configuration.xaml"
            this.DUT_Config.Closing += new System.ComponentModel.CancelEventHandler(this.DUT_Config_Closing);
            
            #line default
            #line hidden
            return;
            case 2:
            this.grd = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.dataGrid_ConfigFile = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 8:
            this.External_devices_configuration = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 9:
            this.dataGrid_GeneratorConfigFile = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 10:
            this.txtStatus = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 11:
            this.Save = ((System.Windows.Controls.Button)(target));
            
            #line 366 "..\..\..\DUT_Configuration.xaml"
            this.Save.Click += new System.Windows.RoutedEventHandler(this.SaveNetConfig_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 4:
            
            #line 98 "..\..\..\DUT_Configuration.xaml"
            ((System.Windows.Controls.ComboBox)(target)).AddHandler(System.Windows.Input.Mouse.MouseMoveEvent, new System.Windows.Input.MouseEventHandler(this.cmb_netPairing_MouseMove));
            
            #line default
            #line hidden
            
            #line 98 "..\..\..\DUT_Configuration.xaml"
            ((System.Windows.Controls.ComboBox)(target)).SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.cmb_netPairing_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 98 "..\..\..\DUT_Configuration.xaml"
            ((System.Windows.Controls.ComboBox)(target)).DropDownOpened += new System.EventHandler(this.cmb_netPairing_DropDownOpened);
            
            #line default
            #line hidden
            break;
            case 5:
            
            #line 102 "..\..\..\DUT_Configuration.xaml"
            ((System.Windows.Controls.TextBlock)(target)).IsMouseDirectlyOverChanged += new System.Windows.DependencyPropertyChangedEventHandler(this.TextBlock_IsMouseDirectlyOverChanged);
            
            #line default
            #line hidden
            break;
            case 6:
            
            #line 167 "..\..\..\DUT_Configuration.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.ID_Click);
            
            #line default
            #line hidden
            break;
            case 7:
            
            #line 234 "..\..\..\DUT_Configuration.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.DateAndTime_Click);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

