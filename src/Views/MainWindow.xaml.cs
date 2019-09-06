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
using AsyncToolWindowSample.Views;
using Grader;
using Microsoft.VisualStudio.Shell;

namespace AsyncToolWindowSample.ToolWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl, IToolWindow
    {
      
        public MainWindow(SampleToolWindowState state)
        {
            InitializeComponent();
            
            pages.Add("Sample", () => new SampleToolWindowControl(state, this));
            pages.Add("Login", () => new LoginView(this));
            pages.Add("ProjectView", () => new ProjectView(new ProjectViewModel(new VisualStudioService(state.AsyncPackage), new ConsoleAppGrader())));
            ToPage("Login");
        }
        private Dictionary<string, Func<Control>> pages = new Dictionary<string, Func<Control>>();
        public void ToPage(string page)
        {
            Content = pages[page]();
        }
    }
}
