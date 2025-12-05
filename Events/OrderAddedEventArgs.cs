using System;
using RestaurantAPI.Models;

namespace RestaurantAPI.Events;

public class OrderAddedEventArgs : EventArgs
{
    public Order Order { get; }

    public OrderAddedEventArgs(Order order)
    {
        Order = order;
    }
}
