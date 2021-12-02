﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBL.BO;

namespace IBL
{
    public partial class BL
    {
        // convertor is between IDAL.DO object and IBL.BO object
        // covnertor1 is between IBL.BO object and IBL.BO object
        private DroneInCharging convertor(IDAL.DO.DroneCharge droneCharge)
        {
            DroneInCharging new_drone = new DroneInCharging();
            DroneToList drone = my_drones.Find(x => x.Id == droneCharge.DroneId);
            new_drone.Battery = drone.Battery;
            new_drone.Id = droneCharge.DroneId;
            return new_drone;
        }
        private BaseStation convertor(IDAL.DO.BaseStation idalBaseStation)
        {
            BaseStation baseStation = new BaseStation
            {
                id = idalBaseStation.Id,
                name = idalBaseStation.Name,
                Num_Free_slots_charge = idalBaseStation.ChargeSlots,
                DroneInChargings = new List<DroneInCharging>() 
            };
            IEnumerable<IDAL.DO.DroneCharge> droneCharges = mydal.Get_all_DroneCharge();
            foreach (var item in droneCharges)
            {
                if (item.StationId == baseStation.id)
                {
                    DroneInCharging drone = convertor(item);
                    baseStation.DroneInChargings.Add(drone);
                }
            }
            baseStation.Num_Free_slots_charge -= baseStation.DroneInChargings.Count();
            if (baseStation.Num_Free_slots_charge < 0)
                throw new DroneChargeException("There more drone from slots");
            Location location = new Location();
            location.latitude = idalBaseStation.Lattitude;
            location.longitude = idalBaseStation.Longitude;
            baseStation.space = location;
            return baseStation;
        }
        private IDAL.DO.BaseStation convertor(BaseStation baseStation)
        {
            IDAL.DO.BaseStation idalBaseStation = new IDAL.DO.BaseStation()
            {
                ChargeSlots = baseStation.Num_Free_slots_charge,
                Id = baseStation.id,
                Lattitude = baseStation.space.latitude,
                Longitude = baseStation.space.longitude,
                Name = baseStation.name
            };
            return idalBaseStation;
        }
        private IDAL.DO.Parcel convertor(Parcel parcel)
        {
            int drone_id;
            if (parcel.droneAtParcel == null)
                drone_id = 0;
            else
                drone_id = parcel.droneAtParcel.Id;
            IDAL.DO.Parcel idalParcel = new IDAL.DO.Parcel
            {
                Delivered = parcel.Delivered,
                Requested = parcel.Requested,
                Scheduled = parcel.Scheduled,
                PickedUp = parcel.PickedUp,
                DroneId = drone_id,
                Priority = (IDAL.DO.Priorities)parcel.priority,
                Weight = (IDAL.DO.WeightCategories)parcel.weight,
                SenderId = parcel.sender.id,
                TargetId = parcel.getter.id
            };
            return idalParcel;
        }
        private Parcel convertor(IDAL.DO.Parcel idalParcel)
        {
            IDAL.DO.Customer idalGetter = mydal.Find_customer(idalParcel.TargetId);
            IDAL.DO.Customer idalSender = mydal.Find_customer(idalParcel.SenderId);
            CustomerAtParcel getter = new CustomerAtParcel()
            {
                customer_name = idalGetter.Name,
                id = idalGetter.Id
            };
            CustomerAtParcel sender = new CustomerAtParcel()
            {
                customer_name = idalSender.Name,
                id = idalSender.Id
            };
            DroneToList drone = my_drones.Find(item => item.Id == idalParcel.Id);
            return new Parcel()
            {
                Delivered = idalParcel.Delivered,
                id = idalParcel.Id,
                PickedUp = idalParcel.PickedUp,
                priority = (IBL.BO.Priorities)idalParcel.Priority,
                weight = (IBL.BO.WeightCategories)idalParcel.Weight,
                Requested = idalParcel.Requested,
                Scheduled = idalParcel.Scheduled,
                getter = getter,
                sender = sender,
                droneAtParcel = convertor1(drone)
            };

        }
        private ParcelAtTransfer convertor1(Parcel parcel)
        {
            Location senderLlocation = new Location();
            Location getter_location = new Location();
            IDAL.DO.Customer sender = mydal.Find_customer(parcel.sender.id);
            IDAL.DO.Customer getter = mydal.Find_customer(parcel.getter.id);
            senderLlocation.longitude = sender.Longitude;
            senderLlocation.latitude = sender.Lattitude;
            getter_location.longitude = sender.Longitude;
            getter_location.latitude = sender.Lattitude;
            return new ParcelAtTransfer()
            {
                id = parcel.id,
                priority = parcel.priority,
                weight = parcel.weight,
                sender = parcel.sender,
                getter = parcel.getter,
                spaceOfPickUp = senderLlocation,
                sateOfParcel = (parcel.PickedUp != DateTime.MinValue),
                distanceOfDelivery = distance_between_2_points(getter_location, senderLlocation),
                spaceOfTarget = getter_location
            };
        }
        private DroneAtParcel convertor1(DroneToList drone)
        {
            return new DroneAtParcel()
            {
                Battery = drone.Battery,
                Id = drone.Id
            };
        }
        private IDAL.DO.Customer convertor(Customer customer)
        {
            IDAL.DO.Customer idalCustomer = new IDAL.DO.Customer()
            {
                Id = customer.id,
                Name = customer.name,
                Lattitude = customer.space.latitude,
                Longitude = customer.space.longitude,
                Phone = customer.phone
            };
            return idalCustomer;
        }
        private IDAL.DO.Drone convertor(Drone drone)
        {
            IDAL.DO.Drone idalDrone = new IDAL.DO.Drone()
            {
                Id = drone.Id,
                MaxWeight = (IDAL.DO.WeightCategories)drone.MaxWeight,
                Model = drone.Model
            };
            return idalDrone;
        }
        private DroneToList convertor1(Drone drone)
        {
            DroneToList droneToList = new DroneToList()
            {
                Id = drone.Id,
                MaxWeight = drone.MaxWeight,
                Model = drone.Model,
                Battery = drone.Battery,
                location = drone.Space,
                Status = drone.Status,
                numOfParcel = 0
            };
            return droneToList;
        }
        private List<BaseStationToList> convertor1(IEnumerable<IDAL.DO.BaseStation> enumerable)
        {
            List<BaseStationToList> baseStations = new List<BaseStationToList>();
            foreach (var item in enumerable)
            {
                baseStations.Add(convertor1(item));
            }
            return baseStations;
        }
        private List<BaseStation> convertor(IEnumerable<IDAL.DO.BaseStation> enumerable)
        {
            List<BaseStation> baseStations = new List<BaseStation>();
            foreach (var item in enumerable)
            {
                baseStations.Add(convertor(item));
            }
            return baseStations;
        }
        private BaseStationToList convertor1(IDAL.DO.BaseStation item)
        {
            List<DroneInCharging> droneCharge = convertor(mydal.Get_all_DroneCharge().ToList().FindAll(charge => charge.StationId == item.Id));
            return new BaseStationToList()
            {
                id = item.Id,
                name = item.Name,
                num_of_busy_slots = droneCharge.Count(),
                num_of_free_slots = item.ChargeSlots
            };
        }
        private List<DroneInCharging> convertor(List<IDAL.DO.DroneCharge> idal_droneCharges)
        {
            List<DroneInCharging> droneInChargings = new List<DroneInCharging>();
            foreach (var item in idal_droneCharges)
            {
                droneInChargings.Add(convertor(item));
            }
            return droneInChargings;
        }
        private List<ParcelToList> convertor1(IEnumerable<IDAL.DO.Parcel> enumerable)
        {
            List<ParcelToList> parcels = new List<ParcelToList>();
            foreach (var item in enumerable)
            {
                parcels.Add(convertor1(item));
            }
            return parcels;
        }
        private List<ParcelToList> convertor(IEnumerable<IDAL.DO.Parcel> enumerable)
        {
            List<ParcelToList> parcels = new List<ParcelToList>();
            foreach (var item in enumerable)
            {
                parcels.Add(convertor1(item));
            }
            return parcels;
        }
        private ParcelToList convertor1(IDAL.DO.Parcel item)
        {
            ParcelStatuses parcelStatuses;
            if (item.Delivered != DateTime.MinValue)
                parcelStatuses = ParcelStatuses.delivered;
            else
            {
                if (item.PickedUp != DateTime.MinValue)
                    parcelStatuses = ParcelStatuses.picked_up;
                else
                {
                    if (item.Scheduled != DateTime.MinValue)
                        parcelStatuses = ParcelStatuses.Belongs;
                    else
                        parcelStatuses = ParcelStatuses.Defined;

                }
            }
            string target_name = mydal.Find_customer(item.TargetId).Name;
            string sender_name = mydal.Find_customer(item.SenderId).Name;
            return new ParcelToList()
            {
                getter = target_name,
                sender = sender_name,
                id = item.Id,
                priority = (IBL.BO.Priorities)item.Priority,
                status = parcelStatuses,
                weight = (IBL.BO.WeightCategories)item.Weight
            };
        }
        private List<CustomerToList> convertor(IEnumerable<IDAL.DO.Customer> enumerable)
        {
            List<CustomerToList> customers = new List<CustomerToList>();
            foreach (var item in enumerable)
            {
                customers.Add(convertor1(item));
            }
            return customers;
        }
        private CustomerToList convertor1(IDAL.DO.Customer item)
        {
            List<IDAL.DO.Parcel> parcels_got = mydal.Get_all_parcels().ToList().FindAll                 (parcel => parcel.TargetId == item.Id && parcel.Delivered != DateTime.MinValue);
            List<IDAL.DO.Parcel> parcels_to_get = mydal.Get_all_parcels().ToList().FindAll              (parcel => parcel.TargetId == item.Id && parcel.Delivered == DateTime.MinValue);
            List<IDAL.DO.Parcel> parcels_sent_and_arrived = mydal.Get_all_parcels().ToList().FindAll    (parcel => parcel.SenderId == item.Id && parcel.Delivered != DateTime.MinValue);
            List<IDAL.DO.Parcel> parcels_sent_and_not_arrived = mydal.Get_all_parcels().ToList().FindAll(parcel => parcel.SenderId == item.Id && parcel.Delivered == DateTime.MinValue);
            return new CustomerToList()
            {
                id = item.Id,
                name = item.Name,
                phone = item.Phone,
                num_of_parcels_got = parcels_got.Count(),
                num_of_parcels_sent_and_arrived = parcels_sent_and_arrived.Count(),
                num_of_parcels_sent_and_not_arrived = parcels_sent_and_not_arrived.Count(),
                num_of_parcels_to_get = parcels_to_get.Count()
            };
        }
        private List<DroneToList> convertor(IEnumerable<IDAL.DO.Drone> enumerable)
        {
            List<DroneToList> drones = new List<DroneToList>();
            foreach (var item in enumerable)
            {
                drones.Add(convertor1(item));
            }
            return drones;
        }
        private DroneToList convertor1(IDAL.DO.Drone item)
        {
            Drone drone = convertor(item);
            List<IDAL.DO.Parcel> parcels = mydal.Get_all_parcels().ToList().FindAll(parcel => parcel.DroneId == item.Id);
            return new DroneToList()
            {
                Battery = drone.Battery,
                Id = drone.Id,
                location = drone.Space,
                MaxWeight = drone.MaxWeight,
                Model = drone.Model,
                Status = drone.Status,
                numOfParcel = parcels.Count()
            };
        }
        private List<Parcel> convertor(List<IDAL.DO.Parcel> idalparcels)
        {
            List<Parcel> parcels = new List<Parcel>();
            foreach (var item in idalparcels)
            {
                parcels.Add(convertor(item));
            }
            return parcels;
        }
        private List<ParcelAtTransfer> convertor1(List<Parcel> parcelsHighPriority)
        {
            List<ParcelAtTransfer> new_list = new List<ParcelAtTransfer>();
            foreach (var item in parcelsHighPriority)
            {
                new_list.Add(convertor1(item));
            }
            return new_list;
        }
        private Drone convertor(IDAL.DO.Drone idalDrone)
        {
            DroneToList droneToList = my_drones.Find(item => item.Id == idalDrone.Id);
            return new Drone()
            {
                Battery = droneToList.Battery,
                Id = droneToList.Id,
                MaxWeight = droneToList.MaxWeight,
                Model = droneToList.Model,
                Space = droneToList.location,
                Status = droneToList.Status
            };
        }
        private Customer convertor(IDAL.DO.Customer idal_customer)
        {
            Location location = new Location();
            location.latitude = idal_customer.Lattitude;
            location.longitude = idal_customer.Longitude;
            Customer customer = new Customer()
            {
                id = idal_customer.Id,
                name = idal_customer.Name,
                phone = idal_customer.Phone,
                space = location
            };
            customer.parcels_at_customer_for = new List<Parcel>();
            customer.parcels_at_customer_from = new List<Parcel>();
            List<IDAL.DO.Parcel> parcels = mydal.Get_all_parcels().ToList();
            foreach (var parcel in parcels)
            {
                if (parcel.SenderId == idal_customer.Id)
                    customer.parcels_at_customer_from.Add(convertor(parcel));
                if (parcel.TargetId == idal_customer.Id)
                    customer.parcels_at_customer_for.Add(convertor(parcel));
            }
            return customer;
        }
    }
}