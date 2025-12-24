using System.Data;

namespace QuanLyCaPhe.Models
{
    public class PaymentAccount
    {
        public int Id { get; set; }
        public string BankName { get; set; }
        public string BankBin { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Template { get; set; }
        public bool IsActive { get; set; }

        public PaymentAccount() { }

        public PaymentAccount(DataRow row)
        {
            Id = (int)row["Id"];
            BankName = row["BankName"].ToString();
            BankBin = row["BankBin"].ToString();
            AccountNumber = row["AccountNumber"].ToString();
            AccountName = row["AccountName"].ToString();
            Template = row["Template"].ToString();
            IsActive = row["IsActive"] != System.DBNull.Value && (bool)row["IsActive"];
        }
    }
}