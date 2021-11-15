﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL.BO
{
    class Customer
    {
        int id;
        string name;
        int phone;
        Space space;
        List<Parcel> parcels_at_customer_from;
        List<Parcel> parcels_at_customer_for;
    }
}
