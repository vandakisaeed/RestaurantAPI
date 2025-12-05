namespace RestaurantAPI.Models
{
    public enum OrderType
    {
        InRestaurant,
        Takeout
    }

    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public OrderType Type { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        // Date-only value used for file partitioning (date component of Timestamp)
        public DateTime Date { get; set; }

        public User? User { get; set; }
    }
}
