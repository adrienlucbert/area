using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Area.API.Models.Table.ManyToMany;
using Area.API.Models.Table.Owned;
using Microsoft.AspNetCore.Identity;

namespace Area.API.Models.Table
{
    public class UserModel : IdentityUser<int>
    {
        public enum UserType {
            Area,
            Facebook,
            Google,
            Microsoft
        }

        public class RoleModel : IdentityRole<int>
        { }

        [ForeignKey("UserId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        public override string UserName { get; set; } = null!;

        public override string Email { get; set; } = null!;

        public UserType Type { get; set; } = UserType.Area;

        public ICollection<UserWidgetModel> Widgets { get; set; } = null!;

        public ICollection<UserServiceTokensModel> ServiceTokens { get; set; } = null!;

        public ICollection<UserParamModel> WidgetParams { get; set; } = null!;

        public ICollection<UserDeviceModel> Devices { get; set; } = null!;
    }
}