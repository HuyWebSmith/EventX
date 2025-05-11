using System.ComponentModel.DataAnnotations;

namespace EventX.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (DisplayAttribute)Attribute.GetCustomAttribute(field, typeof(DisplayAttribute));
            return attribute != null ? attribute.Name : value.ToString();
        }
    }
}
