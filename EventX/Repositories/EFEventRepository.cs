using EventX.Models;
using Microsoft.EntityFrameworkCore;

namespace EventX.Repositories
{
    public class EFEventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;
        public EFEventRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Event>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync(); 
            return await _context.Event
             .Include(e => e.Tickets)
             .Include(e => e.EventImages)
            .Include(p => p.Category) // Include thông tin về category 
            .ToListAsync();

        }

        public async Task<List<Event>> GetByIdAsync(string organizerId)
        {
            return await _context.Event
                .Where(e => e.OrganizerId == organizerId)
                .Include(e => e.EventImages)  // Bao gồm thông tin hình ảnh
                .Include(e => e.Locations)    // Bao gồm thông tin địa điểm
                .ToListAsync();
        }

        public async Task AddAsync(Event events)
        {
            _context.Event.Add(events);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event events)
        {
            _context.Event.Update(events);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var events = await _context.Event.FindAsync(id);
            if (events != null)
            {
                _context.Event.Remove(events);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveImagesByMenuItemIdAsync(int eventId)
        {
            var images = _context.EventImage
           .Where(img => img.EventID == eventId)
           .ToList();

            if (images.Any())
            {
                _context.EventImage.RemoveRange(images);
                await _context.SaveChangesAsync();
            }
        }


    }
}
