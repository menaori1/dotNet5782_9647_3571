﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using BlApi;
using BO;
using System.Runtime.CompilerServices;


namespace BL
{
    sealed partial class BL : IBL
    {
        static internal DalApi.IDal mydal;
        static List<DroneToList> my_drones;
        static double Electricity_free;
        static double Electricity_light;
        static double Electricity_medium;
        static double Electricity_heavy;
        static double Charge_at_second;


        static readonly BL instance = new BL();
        // The public Instance property to use 
        public static BL Instance { get { return instance; } }

        // Explicit static constructor to ensure instance initialization
        // is done just before first usage
        static BL()
        {

            my_drones = new List<DroneToList>();
            mydal = DalApi.DalFactory.GetDal();
            lock (mydal)
            {
                double[] Electricity = mydal.ElectricityUse();
                Electricity_free = Electricity[0];
                Electricity_light = Electricity[1];
                Electricity_medium = Electricity[2];
                Electricity_heavy = Electricity[3];
                Charge_at_second = Electricity[4];
                Random random = new Random();

                #region List of drone from the data layer
                List<DO.Drone> idalDrones = mydal.Get_all_drones().ToList();
                foreach (var item in idalDrones)
                {
                    my_drones.Add(new DroneToList
                    {
                        Id = item.Id,
                        Model = item.Model,
                        MaxWeight = (WeightCategories)item.MaxWeight,
                        NumOfParcel = 0,
                        // Status = (DroneStatuses)random.Next(0, 2)
                        Status = DroneStatuses.vacant
                    });
                }

                #endregion

                #region List of the parcels
                List<DO.Parcel> idalParcel = mydal.Get_all_parcels(x => true).ToList();
                #endregion

                #region List of customer from the data layer
                List<Customer> customers = convertor1(mydal.Get_all_customers()).ToList();
                #endregion

                #region List of base station from the data layer
                List<BaseStation> baseStations = convertor(mydal.Get_all_base_stations(x => true));
                #endregion

                #region List of drone charge from the data layer
                List<DO.DroneCharge> droneCharges = mydal.Get_all_DroneCharge().ToList();
                #endregion

                #region update status of the drone at charging
                foreach (var item in droneCharges)
                {
                    DroneToList drone = my_drones.Find(drone => item.DroneId == drone.Id);
                    drone.Status = DroneStatuses.maintenance;
                    drone.DroneLocation = baseStations.Find(bs => item.StationId == bs.Id).BaseStationLocation;
                    drone.Battery = random.Next(0, 21);
                }
                #endregion


                foreach (var drone in my_drones)
                {
                    List<DO.Parcel> parcel_of_this_drone = idalParcel.FindAll(x => x.DroneId == drone.Id);
                    List<DO.Parcel> parcel_of_this_drone_Delivered = parcel_of_this_drone.FindAll(x => x.Delivered != null);
                    drone.NumOfParcel = parcel_of_this_drone.Count();

                    #region check if the drone is sending
                    if (parcel_of_this_drone.Count() - parcel_of_this_drone_Delivered.Count() != 0)
                        drone.Status = DroneStatuses.sending;
                    #endregion
                    // choose location and random the battery of each drone
                    DO.Parcel parcel = parcel_of_this_drone.Find(x => x.Delivered == null);
                    if (parcel.Id != default(int))
                    {
                        {
                            Customer sender = find_customer(customers, parcel.SenderId);
                            Customer getter = find_customer(customers, parcel.TargetId);
                            BaseStation baseStation_neer_geeter = BaseStation_close_to_location(baseStations, getter.CustomerLocation);
                            if (parcel.Scheduled != null && parcel.PickedUp == null)
                            {
                                BaseStation baseStation_neer_sender = BaseStation_close_to_location(baseStations, sender.CustomerLocation);
                                drone.DroneLocation = baseStation_neer_sender.BaseStationLocation;

                                double distance1 = distance_between_2_points(baseStation_neer_sender.BaseStationLocation, sender.CustomerLocation);
                                double distance2 = distance_between_2_points(sender.CustomerLocation, getter.CustomerLocation);
                                double distance3 = distance_between_2_points(baseStation_neer_geeter.BaseStationLocation, getter.CustomerLocation);
                                double min_battery = (distance1 + distance3) * Electricity_free;
                                switch (parcel.Weight)
                                {
                                    case DO.WeightCategories.light:
                                        min_battery += distance2 * Electricity_light;
                                        break;
                                    case DO.WeightCategories.medium:
                                        min_battery += distance2 * Electricity_medium;
                                        break;
                                    case DO.WeightCategories.heavy:
                                        min_battery += distance2 * Electricity_heavy;
                                        break;
                                    default:
                                        break;
                                }
                                drone.Battery = random.Next((int)min_battery + 1, 101);
                            }
                            if (parcel.PickedUp != null)
                            {
                                drone.DroneLocation = sender.CustomerLocation;
                                double distance1 = distance_between_2_points(sender.CustomerLocation, getter.CustomerLocation);
                                double distance2 = distance_between_2_points(getter.CustomerLocation, baseStation_neer_geeter.BaseStationLocation);
                                double min_battery = distance2 * Electricity_free;
                                switch (parcel.Weight)
                                {
                                    case DO.WeightCategories.light:
                                        min_battery += distance1 * Electricity_light;
                                        break;
                                    case DO.WeightCategories.medium:
                                        min_battery += distance1 * Electricity_medium;
                                        break;
                                    case DO.WeightCategories.heavy:
                                        min_battery += distance1 * Electricity_heavy;
                                        break;
                                    default:
                                        break;
                                }
                                drone.Battery = random.Next((int)min_battery + 1, 101);
                            }
                        }
                    }
                    // cases all of this drone have dlivered
                    // case status is vacant
                    if (drone.Status == DroneStatuses.vacant)
                    {
                        Location location = new Location();
                        // case there are parcels connected to this drone
                        if (parcel_of_this_drone_Delivered.Count() > 0)
                        {
                            int i = random.Next(0, parcel_of_this_drone_Delivered.Count());
                            location = find_customer(customers, parcel_of_this_drone_Delivered[i].TargetId).CustomerLocation;
                        }
                        // case there are not parcels connected to this drone
                        // rand location
                        else
                        {
                            location.Latitude = rand.Next(35160443, 35252793) * 0.000001;
                            location.Longitude = rand.Next(31727247, 31844377) * 0.000001;
                        }
                        drone.DroneLocation = location;
                        BaseStation baseStation_neer_geeter = BaseStation_close_to_location(baseStations, location);
                        double distance = distance_between_2_points(location, baseStation_neer_geeter.BaseStationLocation);
                        double min_battery = distance * Electricity_free;
                        drone.Battery = random.Next((int)min_battery + 1, 101);
                        drone.Battery = drone.Battery;
                    }
                }
            }
        }


        BL() { } // default => private

        /// <summary>
        /// A function that searches for the nearest baseStation from the list to a specific location
        /// </summary>
        /// <param name="baseStations"> list of baseStations  </param>
        /// <param name="myLocation"> the location that  The location for which we are looking for a base station close to it </param>
        /// <returns> the nearest baseStation from the list to the location</returns>
        private static BaseStation BaseStation_close_to_location(List<BaseStation> baseStations, Location myLocation)
        {
            if (baseStations.Count == 0)
                throw new BaseStationExeption("The list is empty");
            BaseStation baseStation = baseStations[0];
            double min_distance = distance_between_2_points(baseStation.BaseStationLocation, myLocation);
            foreach (var item in baseStations)
            {
                double distance = distance_between_2_points(item.BaseStationLocation, myLocation);
                if (distance < min_distance)
                {
                    min_distance = distance;
                    baseStation = item;
                }
            }
            return baseStation;


        }
        /// <summary>
        /// A function that calculates  distance between 2 locations
        /// </summary>
        /// <param name="location1"> the first location </param>
        /// <param name="location2">the second location</param>
        /// <returns></returns>
        private static double distance_between_2_points(Location location1, Location location2)
        {
            double latitude = (location1.Latitude - location2.Latitude) * (location1.Latitude - location2.Latitude);
            double longitude = (location1.Longitude - location2.Longitude) * (location1.Longitude - location2.Longitude);
            return Math.Sqrt(latitude + longitude);
        }
        /// <summary>
        ///  find customer in list by id
        /// </summary>
        /// <param name="customers"> list of customers </param>
        /// <param name="customerId">the id of the customer that we searches </param>
        /// <returns></returns>
        private static Customer find_customer(List<Customer> customers, int customerId)
        {
            foreach (var item in customers)
            {
                if (item.Id == customerId)
                    return item;
            }
            throw new CustomerExeption("id not found");
        }

    }
}