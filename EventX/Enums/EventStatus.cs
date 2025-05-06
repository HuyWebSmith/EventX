using System.ComponentModel.DataAnnotations;

namespace EventX.Enums
{
    public enum EventStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending,

        [Display(Name = "Đã duyệt")]
        Approved,

        [Display(Name = "Bị từ chối")]
        Rejected,

        [Display(Name = "Lên lịch")]
        Scheduled,

        [Display(Name = "Đang diễn ra")]
        Ongoing,

        [Display(Name = "Đã kết thúc")]
        Completed,

        [Display(Name = "Đã hủy")]
        Cancelled
    }

    public static class EventStatusExtensions
    {
        public static string ToVietnamese(this EventStatus status)
        {
            switch (status)
            {
                case EventStatus.Pending:
                    return "Chờ duyệt";
                case EventStatus.Approved:
                    return "Đã duyệt";
                case EventStatus.Rejected:
                    return "Bị từ chối";
                case EventStatus.Scheduled:
                    return "Lên lịch";
                case EventStatus.Ongoing:
                    return "Đang diễn ra";
                case EventStatus.Completed:
                    return "Đã kết thúc";
                case EventStatus.Cancelled:
                    return "Đã hủy";
                default:
                    return string.Empty;
            }
        }
    }
}
