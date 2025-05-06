using System.ComponentModel.DataAnnotations;

public enum BusinessType
{
    [Display(Name = "Công ty tổ chức sự kiện")]
    EventCompany = 1,

    [Display(Name = "Trường học")]
    School = 2,

    [Display(Name = "Doanh nghiệp tư nhân")]
    PrivateBusiness = 3,

    [Display(Name = "Cơ quan nhà nước")]
    GovernmentAgency = 4,

    [Display(Name = "Tổ chức phi lợi nhuận")]
    NonProfitOrganization = 5,

    [Display(Name = "Cá nhân tổ chức sự kiện")]
    Individual = 6
}
