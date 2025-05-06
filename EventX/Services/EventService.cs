using EventX.Enums;
using EventX.Models;

public class EventService
{
    private readonly ApplicationDbContext _context;

    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void UpdateEventStatus()
    {
        var currentDate = DateTime.Now;

        // Tìm các sự kiện đã kết thúc nhưng chưa được đánh dấu là hoàn thành
        var completedEvents = _context.Event
                                      .Where(e => e.EventEndTime < currentDate && e.Status != EventStatus.Completed)
                                      .ToList();
        foreach (var eventItem in completedEvents)
        {
            eventItem.Status = EventStatus.Completed;  // Cập nhật trạng thái hoàn thành
        }

        // Tìm các sự kiện đang diễn ra (thời gian hiện tại nằm trong khoảng bắt đầu và kết thúc)
        var ongoingEvents = _context.Event
                                     .Where(e => e.EventStartTime <= currentDate && e.EventEndTime >= currentDate && e.Status != EventStatus.Ongoing)
                                     .ToList();
        foreach (var eventItem in ongoingEvents)
        {
            eventItem.Status = EventStatus.Ongoing;  // Cập nhật trạng thái đang diễn ra
        }

        _context.SaveChanges();  // Lưu lại thay đổi
    }
}
