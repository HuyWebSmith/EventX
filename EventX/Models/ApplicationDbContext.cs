using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventX.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
       
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
        {
            
        }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<EventImage> EventImage { get; set; }

        public DbSet<Location> Location { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }

        public DbSet<PaymentInfo> PaymentInfos { get; set; }
        public DbSet<RedInvoice> RedInvoices { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<IssuedTicket> IssuedTickets { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany() // Nếu Category có mối quan hệ ngược lại, bạn có thể thay đổi nó
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa Event khi xóa Category
            modelBuilder.Entity<OrderDetail>()
               .HasOne(od => od.Ticket)
               .WithMany()  // Đảm bảo Ticket không có quan hệ ngược lại
               .HasForeignKey(od => od.TicketID)
               .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
