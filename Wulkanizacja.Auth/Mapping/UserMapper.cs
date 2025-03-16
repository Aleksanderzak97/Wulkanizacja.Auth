using Wulkanizacja.Auth.Models;
using Wulkanizacja.Auth.PostgreSQL.Entities;

namespace Wulkanizacja.Auth.Mapping
{
    public static class UserMapper
    {
        public static UserRecord ToUserRecord(this LoginModel model)
        {
            return new UserRecord
            {
                Username = model.Username,
                Password = model.Password
            };
        }

        public static UserRecord ToUserRecord(this RegisterModel model)
        {
            return new UserRecord
            {
                Name = model.Name,
                LastName = model.LastName,
                Username = model.Username,
                Password = model.Password,
                Email = model.Email
            };
        }
    }
}
