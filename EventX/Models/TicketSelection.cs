namespace EventX.Models
{
    public class TicketSelection
    {
        public List<TicketItem> Items { get; set; } = new List<TicketItem>();

        public void AddTicket(TicketItem item)
        {
            var existing = Items.FirstOrDefault(t => t.TicketId == item.TicketId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                Items.Add(item);
        }

        public void RemoveTicket(int ticketId)
        {
            Items.RemoveAll(t => t.TicketId == ticketId);
        }

        public decimal TotalPrice => Items.Sum(t => t.Quantity * t.Price);

    }
}
