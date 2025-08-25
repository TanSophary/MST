using Microsoft.AspNetCore.Identity;

namespace MST.Models
{
    public class Users : IdentityUser
    {
        public String FullName { set; get; }
    }
}
