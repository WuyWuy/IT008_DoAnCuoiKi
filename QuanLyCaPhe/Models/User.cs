using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyCaPhe.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string RoleName { get; set; }
        public bool RoleLevel { get; set; }
        public bool IsActive { get; set; }
        public decimal HourlyWage { get; set; } // new property

        public User() { }

        public User(DataRow row)
        {
            this.Id = (int)row["Id"];
            this.FullName = row["FullName"].ToString();
            this.Email = row["Email"].ToString();

            this.Phone = row["Phone"] != DBNull.Value ? row["Phone"].ToString() : "";
            this.Address = row["Address"] != DBNull.Value ? row["Address"].ToString() : "";
            this.Gender = row["Gender"] != DBNull.Value ? row["Gender"].ToString() : "";

            this.PasswordHash = row["PasswordHash"].ToString();
            this.PasswordSalt = row["PasswordSalt"].ToString();

            this.CreatedAt = (DateTime)row["CreatedAt"];

            this.RoleName = row["RoleName"].ToString();

            this.RoleLevel = (bool)row["RoleLevel"];
            this.IsActive = (bool)row["IsActive"];

            // HourlyWage may not exist in older databases; check column
            if (row.Table.Columns.Contains("HourlyWage") && row["HourlyWage"] != DBNull.Value)
            {
                this.HourlyWage = Convert.ToDecimal(row["HourlyWage"]);
            }
            else
            {
                this.HourlyWage =0m;
            }
        }
    }

}