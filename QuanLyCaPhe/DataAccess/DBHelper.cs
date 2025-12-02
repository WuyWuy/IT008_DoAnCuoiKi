using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration; 

namespace QuanLyCaPhe.DataAccess
{
    public class DBHelper
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["CoffeeDB"]?.ConnectionString
                   ?? "Server=.;Database=QuanLyCaPhe;Trusted_Connection=True;"; 
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
        {
            DataTable data = new DataTable();
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(data);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi Database ExecuteQuery: " + ex.Message);
                }
            }
            return data;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            int rowsAffected = 0;
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        rowsAffected = cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi Database ExecuteNonQuery: " + ex.Message);
                }
            }
            return rowsAffected;
        }

        public static object ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            object? result = null;
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null) cmd.Parameters.AddRange(parameters);
                        result = cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi Database ExecuteScalar: " + ex.Message);
                }
            }
            return result;
        }
    }
}