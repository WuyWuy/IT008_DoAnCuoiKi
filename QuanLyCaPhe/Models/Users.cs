using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    class Users
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int Gender { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string CreateAt { get; set; }
        public string RoleName { get; set; }
        public bool RoleLevel { get; set; }
        public bool IsActive { get; set; }


    }
}
