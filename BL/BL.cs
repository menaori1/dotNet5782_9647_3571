﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using IBL.BO;

namespace IBL
{
    public partial class BL
    {
        static IDAL.IDal mydal;
        List<DroneToList> my_drones;
        double Electricity_free;
        double Electricity_light;
        double Electricity_medium;
        double Electricity_heavy;
        double Charge_at_hour;
        public BL() 
        {
            my_drones = new List<DroneToList>();
            mydal = new DalObject.DalObject();
            double[] Electricity = mydal.ElectricityUse();
            Electricity_free = Electricity[0];
            Electricity_light = Electricity[1];
            Electricity_medium = Electricity[2];
            Electricity_heavy = Electricity[3];
            Charge_at_hour = Electricity[4];
            Random random = new Random();


            #region List of drone from the data layer
             //my_drones = convertor(mydal.Get_all_drones());
            List<IDAL.DO.Drone> idalDrones = mydal.Get_all_drones().ToList();
            foreach (var item in idalDrones)
            {
                my_drones.Add(new DroneToList
                {
                    Id = item.Id,
                    Model = item.Model,
                    MaxWeight = (WeightCategories)item.MaxWeight,
                    NumOfParcel = 0,
                    Status = (DroneStatuses)random.Next(0, 2)
                });
            }
            
            #endregion

            #region List of the parcels
            List<IDAL.DO.Parcel> idalParcel = mydal.Get_all_parcels().ToList();
            #endregion

            #region List of customer from the data layer
            List<Customer> customers = convertor1(mydal.Get_all_customers());
            /*
            List<IDAL.DO.Customer> idalCustomer = mydal.Get_all_customers().ToList();
            List<Customer> customers = new List<Customer>();
            foreach (var item in idalCustomer)
            {
                Location location = new Location();
                location.latitude = item.Lattitude;
                location.longitude = item.Longitude;
                customers.Add(new Customer
                {
                    id = item.Id,
                    name = item.Name,
                    TheLocation = location,
                    phone = item.Phone,
                    parcels_at_customer_for = new List<BO.Parcel>(),
                    parcels_at_customer_from = new List<BO.Parcel>()
                });
            }
            */
            #endregion

            #region List of base station from the data layer
            List<BaseStation> baseStations = convertor(mydal.Get_all_base_stations());
            /*
            List<IDAL.DO.BaseStation> idalBaseStation = mydal.Get_all_base_stations().ToList();
            List<BaseStation> baseStations1 = new List<BaseStation>();
            foreach (var item in idalBaseStation)
            {
                Location location = new Location();
                location.latitude = item.Lattitude;
                location.longitude = item.Longitude;
                baseStations.Add(new BaseStation()
                {
                    id = item.Id,
                    name = item.Name,
                    Num_Free_slots_charge = item.ChargeSlots,
                    space = location,
                    DroneInChargings = new List<DroneInCharging>()
                });
            }
            */
            #endregion


            foreach (var drone in my_drones)
            {
                List<IDAL.DO.Parcel> parcel_of_this_drone = idalParcel.FindAll(x => x.DroneId == drone.Id);
                List<IDAL.DO.Parcel> parcel_of_this_drone_Delivered = parcel_of_this_drone.FindAll(x => x.Delivered != DateTime.MinValue);
                drone.NumOfParcel = parcel_of_this_drone.Count();
                if (parcel_of_this_drone.Count() - parcel_of_this_drone_Delivered.Count() != 0)
                    drone.Status = DroneStatuses.sending;

                // choose location and random the battery ofeach drone
                if(parcel_of_this_drone.Count()>0)
                foreach (var parcel in parcel_of_this_drone)
                {
                    // case the parcel has not delivere
                    if (parcel.Delivered == DateTime.MinValue)
                    {
                        Customer sender = find_customer(customers, parcel.SenderId);
                        Customer getter = find_customer(customers, parcel.TargetId);
                        BaseStation baseStation_neer_geeter = BaseStation_close_to_location(baseStations, getter.CustomerLocation);
                        if (parcel.Scheduled != DateTime.MinValue && parcel.PickedUp == DateTime.MinValue)
                        {
                            BaseStation baseStation_neer_sender = BaseStation_close_to_location(baseStations, sender.CustomerLocation);
                            drone.DroneLocation = baseStation_neer_sender.BaseStationLocation;

                            double distance1 = distance_between_2_points(baseStation_neer_sender.BaseStationLocation, sender.CustomerLocation);
                            double distance2 = distance_between_2_points(sender.CustomerLocation, getter.CustomerLocation);
                            double distance3 = distance_between_2_points(baseStation_neer_geeter.BaseStationLocation, getter.CustomerLocation);
                            double min_battery = (distance1 + distance3) * Electricity_free;
                            switch (parcel.Weight)
                            {
                                case IDAL.DO.WeightCategories.light:
                                    min_battery += distance2 * Electricity_light;
                                    break;
                                case IDAL.DO.WeightCategories.medium:
                                    min_battery += distance2 * Electricity_medium;
                                    break;
                                case IDAL.DO.WeightCategories.heavy:
                                    min_battery += distance2 * Electricity_heavy;
                                    break;
                                default:
                                    break;
                            }
                            drone.Battery = random.Next((int)min_battery + 1, 101);
                        }
                        if (parcel.PickedUp != DateTime.MinValue)
                        {
                            drone.DroneLocation = sender.CustomerLocation;
                            double distance1 = distance_between_2_points(sender.CustomerLocation, getter.CustomerLocation);
                            double distance2 = distance_between_2_points(getter.CustomerLocation, baseStation_neer_geeter.BaseStationLocation);
                            double min_battery = distance2 * Electricity_free;
                            switch (parcel.Weight)
                            {
                                case IDAL.DO.WeightCategories.light:
                                    min_battery += distance1 * Electricity_light;
                                    break;
                                case IDAL.DO.WeightCategories.medium:
                                    min_battery += distance1 * Electricity_medium;
                                    break;
                                case IDAL.DO.WeightCategories.heavy:
                                    min_battery += distance1 * Electricity_heavy;
                                    break;
                                default:
                                    break;
                            }
                            drone.Battery = random.Next((int)min_battery + 1, 101);
                        }
                    }

                }
                // cases all of this drine have dlivere
                // case status is maintenance
                if (drone.Status == DroneStatuses.maintenance)
                {
                    int i = random.Next(0, baseStations.Count);
                    drone.DroneLocation = baseStations[i].BaseStationLocation;
                    drone.Battery = random.Next(0, 21);
                }
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
                        location.latitude = rand.Next(35160443, 35252793) * 0.000001;
                        location.longitude = rand.Next(31727247, 31844377) * 0.000001;
                    }
                    drone.DroneLocation = location;
                    BaseStation baseStation_neer_geeter = BaseStation_close_to_location(baseStations, location);
                    double distance = distance_between_2_points(location, baseStation_neer_geeter.BaseStationLocation);
                    double min_battery = distance * Electricity_free;
                    drone.Battery = random.Next((int)distance + 1, 101);
                }


            }
        }
        /// <summary>
        /// A function that searches for the nearest baseStation from the list to a specific location
        /// </summary>
        /// <param name="baseStations"> list of baseStations  </param>
        /// <param name="myLocation"> the location that  The location for which we are looking for a base station close to it </param>
        /// <returns> the nearest baseStation from the list to the location</returns>
        private BaseStation BaseStation_close_to_location(List<BaseStation> baseStations, Location myLocation)
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
        private double distance_between_2_points(Location location1, Location location2)
        {
            double latitude = (location1.latitude - location2.latitude) * (location1.latitude - location2.latitude);
            double longitude = (location1.longitude - location2.longitude) * (location1.longitude - location2.longitude);
            return Math.Sqrt(latitude + longitude);
        }
        /// <summary>
        ///  find customer in list by id
        /// </summary>
        /// <param name="customers"> list of customers </param>
        /// <param name="customerId">the id of the customer that we searches </param>
        /// <returns></returns>
        private Customer find_customer(List<Customer> customers, int customerId)
        {
            foreach (var item in customers)
            {
                if (item.Id == customerId)
                    return item;
            }
            throw new CustomerExeption("id not found");
        }
    }

    [Serializable]
    internal class slotException : Exception
    {
        public slotException()
        {
        }

        public slotException(string message) : base(message)
        {
        }

        public slotException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected slotException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}