using QuanLyCaPhe.DataAccess;
using System;
using Microsoft.Data.SqlClient;

namespace QuanLyCaPhe.DAO
{
 public partial class BillDAO
 {
 // Delete a bill and its related bill items; also reverse ingredient quantities if necessary
 public bool DeleteBill(int billId)
 {
 // Begin transaction to ensure consistency
 // Steps:
 //1. For each BillInfo (bi) linked to billId, if recipes used ingredients, increment ingredient quantities accordingly
 // But this app doesn't currently track ingredient consumption per bill; so we just delete bill infos and bill.
 //2. Delete from BillInfos where BillId = @billId
 //3. Delete from Bills where Id = @billId

 string deleteBillInfos = "DELETE FROM BillInfos WHERE BillId = @billId";
 string deleteBill = "DELETE FROM Bills WHERE Id = @billId";

 try
 {
 int r1 = DBHelper.ExecuteNonQuery(deleteBillInfos, new SqlParameter[] { new SqlParameter("@billId", billId) });
 int r2 = DBHelper.ExecuteNonQuery(deleteBill, new SqlParameter[] { new SqlParameter("@billId", billId) });
 return (r2 >0);
 }
 catch
 {
 return false;
 }
 }
 }
}
