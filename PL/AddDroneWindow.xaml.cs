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
using System.Collections.ObjectModel;
using BO;

namespace PL
{
    /// <summary>
    /// Interaction logic for AddDroneWindow.xaml
    /// </summary>
    public partial class AddDroneWindow : Window
    {
        DroneToList newDrone;
        BlApi.IBL bl;
        DronesPage theDronesPage;
        ObservableCollection<DroneToList> drones;
        //public AddDroneWindow(BlApi.IBL theBL, ObservableCollection<DroneToList> myDrones)
        public AddDroneWindow(BlApi.IBL theBL, DronesPage dronesPage)
        {
            InitializeComponent();
            bl = theBL;
            theDronesPage = dronesPage;
            newDrone = new DroneToList();
            this.DataContext = newDrone;
            this.Weight.ItemsSource= Enum.GetValues(typeof(WeightCategories));
        }

        private void AddDrone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = int.Parse(this.Id.Text);
                string model = this.Model.Text;
                WeightCategories weight = (WeightCategories)this.Weight.SelectedItem;
                int baseStation = int.Parse(this.Station.Text);
                Drone drone = new Drone()
                {
                    Id = id,
                    MaxWeight = weight,
                    Model = model
                };
                bl.Add_drone(drone, baseStation);
                if (theDronesPage != null)
                    theDronesPage.refresh();
                MessageBox.Show("Succsess", "Succsess");
                Close();
                /*
                if(theDronesPage != null)
                    theDronesPage.drones.Add(new DroneToList()
                    {
                        Id = drone.Id,
                        MaxWeight = drone.MaxWeight,
                        Model = drone.Model,
                        Battery = drone.Battery,
                        DroneLocation = drone.DroneLocation,
                        Status = drone.Status,
                        NumOfParcel = 0
                    });*/
                

            }
            catch (FormatException ex)
            {
                MessageBox.Show("Wrong input: " + ex.Message, "Wrong input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //ListDroneWindow.
        }
    }
}
