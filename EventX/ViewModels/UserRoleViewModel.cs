namespace EventX.ViewModels
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Roles { get; set; } // Chứa danh sách các role dưới dạng chuỗi
    }
}
