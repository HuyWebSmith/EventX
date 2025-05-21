namespace EventX.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string? UserId { get; set; }  
        public int Rating { get; set; }     // 1 - 5 sao
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Event Event { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
