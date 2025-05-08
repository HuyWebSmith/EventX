using EventX.Models;

namespace EventX.Repositories
{
    public interface IEventRepository
    {
        Task<List<Event>> GetAllAsync();
        Task<List<Event>> GetByIdAsync(string organizerId);
        Task AddAsync(Event events);
        Task UpdateAsync(Event events);
        Task DeleteAsync(int id);
        Task RemoveImagesByMenuItemIdAsync(int eventId);
        Task<Event> GetEventWithTicketsAsync(int eventId);

    }
}
