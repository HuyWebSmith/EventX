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
            // Cập nhật trạng thái chỉ khi nó chưa được đánh dấu là hoàn thành
            eventItem.Status = EventStatus.Completed;
        }

        // Tìm các sự kiện đang diễn ra (thời gian hiện tại nằm trong khoảng bắt đầu và kết thúc)
        // Điều kiện thêm là sự kiện phải được duyệt (Status == Approved)
        var ongoingEvents = _context.Event
                                     .Where(e => e.EventStartTime <= currentDate && e.EventEndTime >= currentDate
                                                 && e.Status == EventStatus.Approved && e.Status != EventStatus.Ongoing)
                                     .ToList();

        foreach (var eventItem in ongoingEvents)
        {
            // Cập nhật trạng thái chỉ khi nó chưa được đánh dấu là đang diễn ra
            eventItem.Status = EventStatus.Ongoing;
        }

        // Lưu lại các thay đổi vào cơ sở dữ liệu
        if (completedEvents.Any() || ongoingEvents.Any())
        {
            _context.SaveChanges();
        }
    }

}
