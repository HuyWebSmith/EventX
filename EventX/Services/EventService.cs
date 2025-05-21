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
            // Nếu chưa đánh dấu là hoàn thành, cập nhật trạng thái
            if (eventItem.Status != EventStatus.Completed)
            {
                eventItem.Status = EventStatus.Completed;
            }

            // Kiểm tra đã gửi thông báo chưa
            bool alreadyNotified = _context.Notifications.Any(n =>
                n.UserId == eventItem.OrganizerId &&
                n.Message.Contains($"Sự kiện \"{eventItem.Title}\" đã kết thúc")
            );

            if (!alreadyNotified)
            {
                var notification = new Notification
                {
                    UserId = eventItem.OrganizerId,
                    Message = $"🚩 Sự kiện \"{eventItem.Title}\" đã kết thúc.",
                    Type = NotificationType.ThongBao,
                    CreatedAt = currentDate,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
            }
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
            var notification = new Notification
            {
                UserId = eventItem.OrganizerId,
                Message = $"🔔 Sự kiện \"{eventItem.Title}\" đang diễn ra.",
                Type = NotificationType.ThongBao,
                CreatedAt = currentDate,
                IsRead = false
            };
            _context.Notifications.Add(notification);
        }

        // Cập nhật trạng thái "Scheduled" cho mọi sự kiện chưa có vé
        var eventsWithNoTickets = _context.Event
            .Where(e => !_context.Tickets.Any(t => t.EventID == e.EventID)
                        && e.Status != EventStatus.Scheduled)
            .ToList();

        foreach (var eventItem in eventsWithNoTickets)
        {
            eventItem.Status = EventStatus.Scheduled;
        }


        // Lưu lại các thay đổi vào cơ sở dữ liệu
        if (completedEvents.Any() || ongoingEvents.Any() || eventsWithNoTickets.Any())
        {
            _context.SaveChanges();
        }
    }

}
