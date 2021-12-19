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
using System.Windows.Shapes;
using IBL.BO;

namespace PL
{
    /// <summary>
    /// Interaction logic for DroneViewWindow.xaml
    /// </summary>
    public partial class DroneViewWindow : Window
    {
        IBL.IBL theBL;
        DroneToList drone;
        public DroneViewWindow(Object ob, IBL.IBL bl)
        {
            theBL = bl;
            InitializeComponent();
            drone = (DroneToList)ob;
            Id.Content = drone.Id;
            Model.Content = drone.Model;
            Weight.Content = drone.MaxWeight;
            Battery.Content = drone.Battery;
            Status.Content = drone.Status;
            Longitude.Content = "Longitude: " + drone.DroneLocation.longitude;
            Latitude.Content = "Latitude: " + drone.DroneLocation.latitude;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddDrone_Click(object sender, RoutedEventArgs e)
        {
           new AddDroneWindow(theBL).Show();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            new OptionsDroneWindow(theBL, drone).Show();
        }
    }
}