namespace EventX.Models
{
    public class TicketCart
    {
        public List<TicketItem> Items { get; set; } = new List<TicketItem>();

        public void AddTicket(TicketItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.TicketId == item.TicketId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        public void RemoveTicket(int ticketId)
        {
            var item = Items.FirstOrDefault(i => i.TicketId == ticketId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public decimal GetTotal()
        {
            return Items.Sum(item => item.Price * item.Quantity);
        }
    }
}
