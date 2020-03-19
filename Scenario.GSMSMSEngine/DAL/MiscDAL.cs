using MySql.Data.MySqlClient;
using Scenario.GSMSMSEngine.Helper;
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
       
        public MySqlConnection OpenOnlineDatabaseConnection()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(ConnectionString.tahir123_sms_security);
                con.Open();
                return con;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
        public MySqlConnection OpenLocalDatabaseConnection()
        {
            try
            {
                MySqlConnection con = new MySqlConnection(ConnectionString.con_string);
                con.Open();
                return con;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<SMSQueue> GetSMSQueue(MySqlConnection con)
        {
            List<SMSQueue> lst = new List<SMSQueue>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "Select * from sms_sms_queue where is_sent='N'";

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
                            isEncoded = Convert.ToInt32(reader["isEncoded"]),
                            isSynchronized = Convert.ToInt32(reader["isSynchronized"]),
                            class_id = 0,
                            section_id = 0,
                        };
                        lst.Add(obj);
                    }
                    reader.Close();
                };
            }
            catch (MySqlException ex)
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
            catch (MySqlException ex)
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
                        cmd.CommandText = "Update sms_sms_queue SET is_sent='Y', updated_date_time=@updated_date_time, sender_com_port=@sender_com_port, sender_cell_no=@sender_cell_no, sms_length=@sms_length where id=@id";
                        cmd.Connection = con;

                        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;
                        cmd.Parameters.Add("@updated_date_time", MySqlDbType.DateTime).Value = obj.updated_date_time;
                        cmd.Parameters.Add("@sender_com_port", MySqlDbType.VarChar).Value = obj.sender_com_port;
                        cmd.Parameters.Add("@sender_cell_no", MySqlDbType.VarChar).Value = obj.sender_cell_no;
                        cmd.Parameters.Add("@sms_length", MySqlDbType.Int32).Value = obj.sms_length;

                        con.Open();
                        i = Convert.ToInt32(cmd.ExecuteNonQuery());
                        con.Close();
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            return i;
        }

        //Online Synchronizer
        public int SynchronizeSMSQueue(MySqlConnection conLocal, MySqlConnection conOnline)
        {
            List<SMSQueue> lst = new List<SMSQueue>();
            int result = 0;
            try
            {
                try
                {
                    //Get all unsynchorinized item from local db
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conLocal;
                        cmd.CommandText = "Select * from sms_sms_queue where is_sent='Y' && isSynchronized=0";

                        MySqlDataReader reader = cmd.ExecuteReader();
                        SMSQueue obj;
                        while (reader.Read())
                        {
                            obj = new SMSQueue()
                            {
                                id = Convert.ToInt32(reader["id"]),
                                updated_date_time = HelperClass.ConvertFromDBVal<DateTime>(reader["updated_date_time"]),
                                downloaded_date_time = HelperClass.ConvertFromDBVal<DateTime>(reader["downloaded_date_time"]),
                            };
                            lst.Add(obj);
                        }
                        reader.Close();
                    };
                }
                catch (Exception ex)
                {
                }

                //UpdateOnlineSMSQueueAndLocalSMSQueueSynchronization
                try
                {
                    foreach (var obj in lst)
                    {
                        using (MySqlCommand cmd = new MySqlCommand())
                        {
                            cmd.CommandText = "Update sms_sms_queue SET is_sent='Y', updated_date_time=@updated_date_time, sender_com_port=@sender_com_port, sender_cell_no=@sender_cell_no, sms_length=@sms_length, downloaded_date_time=@downloaded_date_time, isSynchronized=@isSynchronized where id=@id";
                            cmd.Connection = conOnline;

                            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;
                            cmd.Parameters.Add("@updated_date_time", MySqlDbType.DateTime).Value = obj.updated_date_time;
                            cmd.Parameters.Add("@downloaded_date_time", MySqlDbType.DateTime).Value = obj.downloaded_date_time;
                            cmd.Parameters.Add("@sender_com_port", MySqlDbType.VarChar).Value = obj.sender_com_port;
                            cmd.Parameters.Add("@sender_cell_no", MySqlDbType.VarChar).Value = obj.sender_cell_no;
                            cmd.Parameters.Add("@sms_length", MySqlDbType.Int32).Value = obj.sms_length;
                            cmd.Parameters.Add("@isSynchronized", MySqlDbType.Int32).Value = obj.isSynchronized;

                            result = Convert.ToInt32(cmd.ExecuteNonQuery());
                            cmd.Parameters.Clear();
                        }

                        if (result > 0)
                        {
                            using (MySqlCommand cmd = new MySqlCommand())
                            {
                                cmd.CommandText = "Update sms_sms_queue SET isSynchronized=1 where id=@id";
                                cmd.Connection = conLocal;

                                cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;
                                result = Convert.ToInt32(cmd.ExecuteNonQuery());
                                cmd.Parameters.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                // get all items from online where is_Sent=N
                try
                {
                    lst = new List<SMSQueue>();
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conOnline;
                        cmd.CommandText = "Select * from sms_sms_queue where is_sent='N'";

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
                                sms_length = Convert.ToInt32(reader["sms_length"]),
                                sms_type_id = Convert.ToInt32(reader["sms_type_id"]),
                                created_by = Convert.ToString(reader["created_by"]),
                                emp_id = Convert.ToInt32(reader["emp_id"]),
                                sort_order = Convert.ToInt32(reader["sort_order"]),
                                date_time = HelperClass.ConvertFromDBVal<DateTime>(reader["date_time"]),
                                class_id = 0,
                                section_id = 0,
                                is_sent = Convert.ToString(reader["is_sent"]),
                                is_periority = Convert.ToString(reader["is_periority"]),
                                sender_cell_no = Convert.ToString(reader["sender_cell_no"]),
                                sender_com_port = Convert.ToString(reader["sender_com_port"]),
                                institute_id = Convert.ToInt32(reader["institute_id"]),
                                institute_name = Convert.ToString(reader["institute_name"]),
                                institute_cell = Convert.ToString(reader["institute_cell"]),
                                created_date_time = HelperClass.ConvertFromDBVal<DateTime>(reader["created_date_time"]),
                                downloaded_date_time = HelperClass.ConvertFromDBVal<DateTime>(reader["downloaded_date_time"]),
                                updated_date_time = HelperClass.ConvertFromDBVal<DateTime>(reader["updated_date_time"]),
                                isEncoded = Convert.ToInt32(reader["isEncoded"]),
                                isSynchronized = Convert.ToInt32(reader["isSynchronized"]),
                            };
                            lst.Add(obj);
                        }
                        reader.Close();
                    };
                }
                catch (Exception ex)
                {
                }

                // insert in local database                
                foreach (var obj in lst)
                {
                    try
                    {
                        using (MySqlCommand cmd = new MySqlCommand())
                        {
                            cmd.CommandText = "INSERT INTO sms_sms_queue(id, receiver_id,receiver_type_id, receiver_cell_no, receiver_name, sms_message, sms_type, sms_type_id, sms_length, sort_order, created_by, date_time, emp_id, is_sent, is_periority, sender_cell_no, sender_com_port, institute_id, institute_name, institute_cell, created_date_time, downloaded_date_time) " +
                                "Values(@id, @receiver_id, @receiver_type_id, @receiver_cell_no, @receiver_name, @sms_message, @sms_type, @sms_type_id, @sms_length, @sort_order, @created_by, @date_time, @emp_id, @is_sent, @is_periority, @sender_cell_no, @sender_com_port, @institute_id, @institute_name, @institute_cell, @created_date_time, @downloaded_date_time)";
                            cmd.Connection = conLocal;

                            cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;
                            cmd.Parameters.Add("@receiver_id", MySqlDbType.Int32).Value = obj.receiver_id;
                            cmd.Parameters.Add("@receiver_type_id", MySqlDbType.Int32).Value = obj.receiver_type_id;
                            cmd.Parameters.Add("@receiver_cell_no", MySqlDbType.VarChar).Value = obj.receiver_cell_no;
                            cmd.Parameters.Add("@receiver_name", MySqlDbType.VarChar).Value = obj.receiver_name;
                            cmd.Parameters.Add("@sms_message", MySqlDbType.VarChar).Value = obj.sms_message;
                            cmd.Parameters.Add("@sms_type", MySqlDbType.VarChar).Value = obj.sms_type;
                            cmd.Parameters.Add("@sms_type_id", MySqlDbType.Int32).Value = obj.sms_type_id;
                            cmd.Parameters.Add("@sms_length", MySqlDbType.Int32).Value = obj.sms_length;
                            cmd.Parameters.Add("@sort_order", MySqlDbType.Int32).Value = obj.sort_order;
                            cmd.Parameters.Add("@created_by", MySqlDbType.VarChar).Value = obj.created_by;
                            cmd.Parameters.Add("@date_time", MySqlDbType.DateTime).Value = obj.date_time;
                            cmd.Parameters.Add("@emp_id", MySqlDbType.Int32).Value = obj.emp_id;

                            cmd.Parameters.Add("@is_sent", MySqlDbType.VarChar).Value = obj.is_sent;
                            cmd.Parameters.Add("@is_periority", MySqlDbType.VarChar).Value = obj.is_periority;
                            cmd.Parameters.Add("@sender_cell_no", MySqlDbType.VarChar).Value = obj.sender_cell_no;
                            cmd.Parameters.Add("@sender_com_port", MySqlDbType.VarChar).Value = obj.sender_com_port;
                            cmd.Parameters.Add("@institute_id", MySqlDbType.Int32).Value = obj.institute_id;
                            cmd.Parameters.Add("@institute_name", MySqlDbType.VarChar).Value = obj.institute_name;
                            cmd.Parameters.Add("@institute_cell", MySqlDbType.VarChar).Value = obj.institute_cell;
                            cmd.Parameters.Add("@created_date_time", MySqlDbType.DateTime).Value = obj.created_date_time;
                            cmd.Parameters.Add("@downloaded_date_time", MySqlDbType.DateTime).Value = DateTime.Now;
                            cmd.Parameters.Add("@isSynchronized", MySqlDbType.Int32).Value = obj.isSynchronized;
                            cmd.Parameters.Add("@isEncoded", MySqlDbType.Int32).Value = obj.isEncoded;

                            result = Convert.ToInt32(cmd.ExecuteNonQuery());
                            cmd.Parameters.Clear();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        //To avoid duplicataion
                        //it can be happended that item may be already exists in local db but still not sent
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            return result;
        }
        public int UpdateOnlineSMSQueueAndLocalSMSQueueSynchronization(SMSQueue obj, MySqlConnection conLocal, MySqlConnection conOnline)
        {
            int i = 0;
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.CommandText = "Update sms_sms_queue SET is_sent='Y', updated_date_time=@updated_date_time, downloaded_date_time=@downloaded_date_time where id=@id";
                    cmd.Connection = conOnline;

                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;
                    cmd.Parameters.Add("@updated_date_time", MySqlDbType.DateTime).Value = obj.updated_date_time;
                    cmd.Parameters.Add("@downloaded_date_time", MySqlDbType.DateTime).Value = obj.downloaded_date_time;
                    cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;

                    i = Convert.ToInt32(cmd.ExecuteNonQuery());
                    cmd.Parameters.Clear();
                }

                if (i > 0)
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.CommandText = "Update sms_sms_queue SET isSynchronized=1 where id=@id";
                        cmd.Connection = conLocal;

                        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = obj.id;
                        i = Convert.ToInt32(cmd.ExecuteNonQuery());
                        cmd.Parameters.Clear();
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            return i;
        }
        public List<SMSQueue> GetSMSQueueOnline(MySqlConnection con)
        {
            List<SMSQueue> lst = new List<SMSQueue>();
            try
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = "Select * from sms_sms_queue where is_sent='N'";

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
                            sms_length = Convert.ToInt32(reader["sms_length"]),
                            sms_type_id = Convert.ToInt32(reader["sms_type_id"]),
                            created_by = Convert.ToString(reader["created_by"]),
                            emp_id = Convert.ToInt32(reader["emp_id"]),
                            sort_order = Convert.ToInt32(reader["sort_order"]),
                            date_time = Convert.ToDateTime(reader["date_time"]),
                            class_id = 0,
                            section_id = 0,
                            is_sent = Convert.ToString(reader["is_sent"]),
                            is_periority = Convert.ToString(reader["is_periority"]),
                            sender_cell_no = Convert.ToString(reader["sender_cell_no"]),
                            sender_com_port = Convert.ToString(reader["sender_com_port"]),
                            institute_id = Convert.ToInt32(reader["institute_id"]),
                            institute_name = Convert.ToString(reader["institute_name"]),
                            institute_cell = Convert.ToString(reader["institute_cell"]),
                            created_date_time = Convert.ToDateTime(reader["created_date_time"]),
                            downloaded_date_time = Convert.ToDateTime(reader["downloaded_date_time"]),
                            updated_date_time = Convert.ToDateTime(reader["updated_date_time"]),
                            isEncoded = Convert.ToInt32(reader["isEncoded"]),
                        };
                        lst.Add(obj);
                    }
                    reader.Close();
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return lst;
        }
        public int InsertSMSQueueLocal(List<SMSQueue> lst, MySqlConnection con)
        {
            int i = 0;

            try
            {
                foreach (var obj in lst)
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.CommandText = "INSERT INTO sms_sms_queue(receiver_id,receiver_type_id, receiver_cell_no, receiver_name, sms_message, sms_type, sms_type_id, sms_length, sort_order, created_by, date_time, emp_id, is_sent, is_periority, sender_cell_no, sender_com_port, institute_id, institute_name, institute_cell, created_date_time, downloaded_date_time, isEncoded) " +
                            "Values(@receiver_id, @receiver_type_id, @receiver_cell_no, @receiver_name, @sms_message, @sms_type, @sms_type_id, @sms_length, @sort_order, @created_by, @date_time, @emp_id, @is_sent, @is_periority, @sender_cell_no, @sender_com_port, @institute_id, @institute_name, @institute_cell, @created_date_time, @downloaded_date_time, @isEncoded)";
                        cmd.Connection = con;

                        cmd.Parameters.Add("@receiver_id", MySqlDbType.Int32).Value = obj.receiver_id;
                        cmd.Parameters.Add("@receiver_type_id", MySqlDbType.Int32).Value = obj.receiver_type_id;
                        cmd.Parameters.Add("@receiver_cell_no", MySqlDbType.VarChar).Value = obj.receiver_cell_no;
                        cmd.Parameters.Add("@receiver_name", MySqlDbType.VarChar).Value = obj.receiver_name;
                        cmd.Parameters.Add("@sms_message", MySqlDbType.VarChar).Value = obj.sms_message;
                        cmd.Parameters.Add("@sms_type", MySqlDbType.VarChar).Value = obj.sms_type;
                        cmd.Parameters.Add("@sms_type_id", MySqlDbType.Int32).Value = obj.sms_type_id;
                        cmd.Parameters.Add("@sms_length", MySqlDbType.Int32).Value = obj.sms_length;
                        cmd.Parameters.Add("@sort_order", MySqlDbType.Int32).Value = obj.sort_order;
                        cmd.Parameters.Add("@created_by", MySqlDbType.VarChar).Value = obj.created_by;
                        cmd.Parameters.Add("@date_time", MySqlDbType.DateTime).Value = obj.date_time;
                        cmd.Parameters.Add("@emp_id", MySqlDbType.Int32).Value = obj.emp_id;

                        cmd.Parameters.Add("@is_sent", MySqlDbType.VarChar).Value = obj.is_sent;
                        cmd.Parameters.Add("@is_periority", MySqlDbType.VarChar).Value = obj.is_periority;
                        cmd.Parameters.Add("@sender_cell_no", MySqlDbType.VarChar).Value = obj.sender_cell_no;
                        cmd.Parameters.Add("@sender_com_port", MySqlDbType.VarChar).Value = obj.sender_com_port;
                        cmd.Parameters.Add("@institute_id", MySqlDbType.Int32).Value = obj.institute_id;
                        cmd.Parameters.Add("@institute_name", MySqlDbType.VarChar).Value = obj.institute_name;
                        cmd.Parameters.Add("@institute_cell", MySqlDbType.VarChar).Value = obj.institute_cell;
                        cmd.Parameters.Add("@created_date_time", MySqlDbType.DateTime).Value = obj.created_date_time;
                        cmd.Parameters.Add("@downloaded_date_time", MySqlDbType.DateTime).Value = DateTime.Now;
                        cmd.Parameters.Add("@isEncoded", MySqlDbType.Int32).Value = obj.isEncoded;

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
