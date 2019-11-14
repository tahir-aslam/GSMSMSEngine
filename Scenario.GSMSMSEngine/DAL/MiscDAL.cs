using MySql.Data.MySqlClient;
using Scenario.GSMSMSEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenario.GSMSMSEngine.DAL
{
    public class MiscDAL
    {
        public MiscDAL()
        {
        }

        public List<SMSQueue> GetSMSQueue()
        {
            List<SMSQueue> lst = new List<SMSQueue>();
            try
            {
                using (MySqlConnection con = new MySqlConnection(ConnectionString.con_string))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandText = "Select * from sms_sms_queue where is_sent='N'";
                        con.Open();

                        MySqlDataReader reader = cmd.ExecuteReader();
                        SMSQueue obj;
                        while (reader.Read())
                        {
                            obj = new SMSQueue()
                            {
                                id = Convert.ToInt32(reader["id"]),
                                receiver_id = Convert.ToInt32(reader["receiver_id"]),
                                receiver_name = Convert.ToString(reader["receiver_name"]),
                                receiver_cell_no = Convert.ToString(reader["receiver_cell_no"]),
                                receiver_type_id = Convert.ToInt32(reader["receiver_type_id"]),
                                sms_message = Convert.ToString(reader["sms_message"]),
                                sms_type = Convert.ToString(reader["sms_type"]),
                                sms_type_id = Convert.ToInt32(reader["sms_type_id"]),
                                created_by = Convert.ToString(reader["created_by"]),
                                emp_id = Convert.ToInt32(reader["emp_id"]),                                
                                sort_order = Convert.ToInt32(reader["sort_order"]),
                                date_time = Convert.ToDateTime(reader["date_time"]),
                                class_id = 0,
                                section_id = 0,

                            };
                            lst.Add(obj);
                        }
                    };
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }
        public int InsertSMSHistory(SMSHistory sh)
        {
            int i = 0;
            try
            {
                using (MySqlConnection con = new MySqlConnection(ConnectionString.con_string))
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.CommandText = "INSERT INTO sms_history(sender_id,sender_name,class_id,class_name,section_id,section_name,cell,created_by,date_time,sms_type,msg) Values(@sender_id,@sender_name,@class_id,@class_name,@section_id,@section_name,@cell,@created_by,@date_time,@sms_type,@msg)";
                        cmd.Connection = con;
                        //cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.Add("@sender_id", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.sender_id;
                        cmd.Parameters.Add("@sender_name", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.sender_name;
                        cmd.Parameters.Add("@class_id", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.class_id;
                        cmd.Parameters.Add("@class_name", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.class_name;
                        cmd.Parameters.Add("@section_id", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.section_id;
                        cmd.Parameters.Add("@section_name", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.section_name;
                        cmd.Parameters.Add("@cell", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.cell;
                        cmd.Parameters.Add("@msg", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.msg;
                        cmd.Parameters.Add("@sms_type", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.sms_type;
                        cmd.Parameters.Add("@created_by", MySql.Data.MySqlClient.MySqlDbType.VarChar).Value = sh.created_by;
                        cmd.Parameters.Add("@date_time", MySql.Data.MySqlClient.MySqlDbType.DateTime).Value = sh.date_time;

                        con.Open();
                        i = Convert.ToInt32(cmd.ExecuteNonQuery());
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return i;
        }

        public int UpdateSMSQueue(SMSQueue obj)
        {
             int i = 0;
             try
             {
                 using (MySqlConnection con = new MySqlConnection(ConnectionString.con_string))
                 {
                     using (MySqlCommand cmd = new MySqlCommand())
                     {
                         cmd.CommandText = "Update sms_sms_queue SET is_sent='Y' where id=@id";
                         cmd.Connection = con;
                         con.Open();

                         cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;

                         i = Convert.ToInt32(cmd.ExecuteNonQuery());
                         cmd.Parameters.Clear();
                     }
                 }
             }
             catch (Exception ex)
             {
                 throw ex;
             }
            return i;
        }
        
    }
}
