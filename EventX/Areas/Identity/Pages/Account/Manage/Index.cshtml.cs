// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using EventX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventX.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        public string AvatarUrlPath { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Display(Name = "Date of Birth")]
            public DateTime? DateOfBirth { get; set; }

            [Display(Name = "Avatar")]
            public IFormFile AvatarUrl { get; set; }

            [Display(Name = "Role Preference")]
            public string RolePreference { get; set; }

            [Display(Name = "Interest Area")]
            public string InterestArea { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var fullName = user.FullName;
            var gender = user.Gender;
            var dateOfBirth = user.DateOfBirth;
            var avatarUrl = user.AvatarUrl;
            var rolePreference = user.RolePreference;
            var interestArea = user.InterestArea;
            var email = user.Email;

            Username = userName;
            AvatarUrlPath = avatarUrl ?? "/images/download.png"; 
            Input = new InputModel
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,

                Gender = gender,
                DateOfBirth = dateOfBirth,
                RolePreference = rolePreference,
                InterestArea = interestArea,
                Email = email
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
           

            // Cập nhật thông tin
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }
            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            if (Input.Gender != user.Gender)
            {
                user.Gender = Input.Gender;
            }
            if (Input.DateOfBirth != user.DateOfBirth)
            {
                user.DateOfBirth = Input.DateOfBirth;
            }

            // Lưu Avatar
            if (Input.AvatarUrl != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.AvatarUrl.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                // Lưu ảnh vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.AvatarUrl.CopyToAsync(stream);
                }

                // Cập nhật đường dẫn vào cơ sở dữ liệu
                user.AvatarUrl = "/images/" + fileName;

                // Đảm bảo đường dẫn đã được cập nhật
                Console.WriteLine("Avatar URL: " + user.AvatarUrl);
            }


            if (Input.RolePreference != user.RolePreference)
            {
                user.RolePreference = Input.RolePreference;
            }
            if (Input.InterestArea != user.InterestArea)
            {
                user.InterestArea = Input.InterestArea;
            }
            if (Input.Email != user.Email)
            {
                user.Email = Input.Email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to update user profile.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated.";
            return RedirectToPage();
        }
    }
}
