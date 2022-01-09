﻿using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlApi
{
    public interface IBL
    {
        void Add_base_station(BaseStation baseStation);
        void Add_customer(Customer customer);
        void Add_drone(Drone drone, int baseStation_num);
        void Add_parcel(Parcel parcel, int sender_id, int getter_id);
        void connect_parcel_to_drone(int drone_id);
        void delivered_parcel_by_drone(int drone_id);
        void drone_from_charge(int drone_id);
        void pickedUp_parcel_by_drone(int drone_id);
        void send_drone_to_charge(int id);
        string StringAllCustomers();
        string StringAllDrones();
        string StringAllParcels();
        string StringAllParcelsWithout_drone();
        string StringCustomer(int customer_id);
        string StringDrone(int drone_id);
        IEnumerable<BaseStationToList> GetAllBaseStations(Predicate<BaseStationToList> match);
        IEnumerable<DroneToList> GetAllDrones(Predicate<DroneToList> match);
        IEnumerable<CustomerToList> GetAllCustomers(Predicate<CustomerToList> match);
        string StringParcel(int parcel_id);
        string string_all_baseStations();
        string string_all_baseStations_with_free_slots();
        Drone GetDrone(DroneToList drone);
        string string_baseStation(int baseStation_id);
        void update_baseStation(int id, string new_name, string new_slot);
        void update_customer(int id, string new_name, string new_phone);
        void update_model_drone(int drone_id, string model);
        ParcelAtTransfer GetCurrectParcelAtTransferOfDrone(int id);
        Parcel GetCurrectParcelOfDrone(int id);
    }
}