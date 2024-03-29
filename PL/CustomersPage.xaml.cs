﻿using BO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


namespace PL
{
    /// <summary>
    /// Interaction logic for CustomersPage.xaml
    /// </summary>
    public partial class CustomersPage : Page
    {
        BlApi.IBL bl;
        internal ObservableCollection<CustomerToList> customers;

        public CustomersPage(BlApi.IBL theBL)
        {
            bl = theBL;
            InitializeComponent();
            customers = new ObservableCollection<CustomerToList>(bl.GetAllCustomers(item => true));
            DataContext = customers;
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            new AddCustomerWindow(bl, refresh).Show();
        }
        internal void refresh()
        {
            customers = new ObservableCollection<CustomerToList>(bl.GetAllCustomers(item => true));
            DataContext = customers;
        }

        private void CustomersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (CustomersListView.SelectedItem != null)
            {
                Customer customer =bl.GetCustomer((CustomerToList) CustomersListView.SelectedItem);
                new CustomerViewWindow(customer, bl , refresh, true).Show();
              
            }
            CustomersListView.UnselectAll();
            
        }
    }
}
