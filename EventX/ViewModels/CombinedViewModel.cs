namespace EventX.ViewModels
{
    public class CombinedViewModel
    {
        public OrderListViewModel OrderViewModel { get; set; }
        public TicketListViewModel TicketViewModel { get; set; }
        public int EventID { get; set; }
    }

}