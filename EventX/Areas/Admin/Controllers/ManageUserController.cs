using EventX.Models;
using EventX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace EventX.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManageUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ManageUserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
        public async Task<IActionResult> ManageRoles()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user) ?? new List<string>(); // Lấy role hoặc danh sách rỗng
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = string.Join(", ", roles) // Nối nhiều role thành chuỗi (nếu có)
                });
            }

            return View(userRoles);

        }
        public async Task<IActionResult> EditRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = _roleManager.Roles?.Select(r => r.Name).ToList() ?? new List<string>(); // Tránh null
            var userRoles = await _userManager.GetRolesAsync(user) ?? new List<string>(); // Tránh null
            if (userRoles.Contains("Admin"))
            {
                TempData["Error"] = "Bạn không thể chỉnh sửa vai trò của tài khoản Admin.";
                return RedirectToAction("ManageRoles");
            }
            var model = new EditRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(r => new RoleSelection
                {
                    RoleName = r,
                    Selected = userRoles.Contains(r) // Bây giờ userRoles luôn có giá trị, tránh null
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (model.Roles == null) model.Roles = new List<RoleSelection>();
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.SelectedRoles ?? new List<string>();

            await _userManager.RemoveFromRolesAsync(user, userRoles);
            await _userManager.AddToRolesAsync(user, selectedRoles);

            return RedirectToAction("ManageRoles");
        }

        public async Task<IActionResult> Block(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = "Không thể khóa tài khoản Admin.";
                return RedirectToAction("Index");
            }

            user.IsBlocked = true;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Đã khóa tài khoản thành công.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Người dùng không tồn tại.";
                return RedirectToAction("Index");
            }

            user.IsBlocked = false;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Đã mở khóa tài khoản thành công.";
            return RedirectToAction("Index");
        }

    }
}
