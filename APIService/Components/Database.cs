using APIService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Serialization;

namespace APIService.Components
{
    public static class Database
    {
        private static SqlConnection _connection;

        public enum DatabaseOpsStatus
        {
            SUCESS,
            ERROR
        }

        public static void Connect()
        {
            string connectionString = System.IO.File.ReadAllText("..\\DBConnectionString");
            _connection = new SqlConnection(@connectionString);
            _connection.Open();
        }

        public static void Disconnect()
        {
            _connection.Close();
        }


        #region Nudges
        public static void UpdateNudgeEventReferences(ref Nudge nudge)
        {
            string addQuery = "insert into eventnudges (nid,eid) values";
            string deleteQuery = "delete from eventnudges where nid="+nudge.Id.ToString()+" AND eid in (";

            int deleteCount = 0;
            int addCount = 0;
            for (int i = 0; i < nudge.Events.Length; i++)
            {
                if (nudge.Events[i].Id < 0)
                {
                    deleteQuery += Math.Abs(nudge.Events[i].Id) + ",";
                    deleteCount++;
                }
                else
                {
                    addQuery += "(" + nudge.Id.ToString() + "," + nudge.Events[i].Id.ToString() + "),";
                    addCount++;
                }
            }
            
            deleteQuery= deleteQuery.Remove(deleteQuery.Length-1,1);
            deleteQuery += ");";

            addQuery = addQuery.Remove(addQuery.Length-1,1);
            addQuery += ";";

            bool addStatus = true;
            bool deleteStatus = true;
            if(addCount >0)
            {
                SqlCommand addCommand = new SqlCommand(addQuery, _connection);
                addStatus = addCommand.ExecuteNonQuery() == addCount ? true : false;
            }

            if(deleteCount >0) 
            {
                SqlCommand deleteCommand = new SqlCommand(deleteQuery, _connection);
                deleteStatus = deleteCommand.ExecuteNonQuery() == deleteCount ? true : false;
            }

            if(addStatus && deleteStatus)
            {
                nudge.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                nudge.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void AddNudge(ref Nudge nudge)
        {
            SqlCommand command = new SqlCommand("insert into nudges(title,invitation,base64cover,base64icon,schedule) values(@title,@invitation,@base64cover,@base64icon,@schedule);SELECT CAST(scope_identity() AS int);", _connection);

            command.Parameters.Add("title", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("invitation", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("base64cover", System.Data.SqlDbType.Text);
            command.Parameters.Add("base64icon", System.Data.SqlDbType.Text);
            command.Parameters.Add("schedule", System.Data.SqlDbType.DateTime);

            command.Parameters["title"].Value = nudge.Title;
            command.Parameters["invitation"].Value = nudge.Invitation;
            command.Parameters["base64cover"].Value = nudge.Base64Cover;
            command.Parameters["base64icon"].Value = nudge.Base64Cover;
            command.Parameters["schedule"].Value = nudge.Schedule;

            object result = command.ExecuteScalar();
            if (result != null)
            {
                nudge.Id = Convert.ToInt32(result);
                nudge.DBStatus = DatabaseOpsStatus.SUCESS;
                if(nudge.Events.Length > 0)
                    UpdateNudgeEventReferences(ref nudge);
            }
            else
            {
                nudge.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void UpdateNudge(ref Nudge nudge) 
        {
            SqlCommand command = new SqlCommand("update nudges set title=@title,invitation=@invitation,base64cover=@base64cover,base64icon=@base64icon,schedule=@schedule where id=@id;", _connection);

            command.Parameters.Add("id", System.Data.SqlDbType.Int);
            command.Parameters.Add("title", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("invitation", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("base64cover", System.Data.SqlDbType.Text);
            command.Parameters.Add("base64icon", System.Data.SqlDbType.Text);
            command.Parameters.Add("schedule", System.Data.SqlDbType.DateTime);

            command.Parameters["id"].Value = nudge.Id;
            command.Parameters["title"].Value = nudge.Title;
            command.Parameters["invitation"].Value = nudge.Invitation;
            command.Parameters["base64cover"].Value = nudge.Base64Cover;
            command.Parameters["base64icon"].Value = nudge.Base64Cover;
            command.Parameters["schedule"].Value = nudge.Schedule;

            command.ExecuteNonQuery();

            if (nudge.Events.Length > 0)
                UpdateNudgeEventReferences(ref nudge);
            else
                nudge.DBStatus = DatabaseOpsStatus.SUCESS;
        }

        public static void DeleteNudge(ref Nudge nudge)
        {
            SqlCommand command = new SqlCommand("delete from eventnudges where nid="+nudge.Id+";", _connection);
            command.ExecuteNonQuery();
            
            command = new SqlCommand("delete from nudges where id="+nudge.Id+";", _connection);
            int rows = command.ExecuteNonQuery();

            if (rows > 0)
                nudge.DBStatus = DatabaseOpsStatus.SUCESS;
            else
                nudge.DBStatus = DatabaseOpsStatus.ERROR;
        }

        public static List<Nudge> GetEventNudges(ref Event _event)
        {
            List<Nudge> nudges = new List<Nudge>();
            SqlCommand command = new SqlCommand("select nid from eventnudges where eid=@id;", _connection);

            command.Parameters.Add("id", System.Data.SqlDbType.Int);
            command.Parameters["id"].Value = _event.Id;

            List<int> nudgeIds = new List<int>();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                nudgeIds.Add((int)reader["nid"]);
            }
            reader.Close();
                        
            foreach(int id in nudgeIds)
            {
                Nudge nudge = Database.GetNudge(id);
                if(nudge.Id > -1)
                {
                    nudges.Add(nudge);
                }
            }

            return nudges;
        }

        public static void GetNudgeEvents(ref Nudge nudge)
        {
            SqlCommand command = new SqlCommand("select eid from eventnudges where nid=@id;", _connection);

            command.Parameters.Add("id", System.Data.SqlDbType.Int);
            command.Parameters["id"].Value = nudge.Id;

            List<int> eventIds = new List<int>();
            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                eventIds.Add((int)reader["eid"]);
            }
            reader.Close();

            List<Event> events = new List<Event>();
            foreach (int id in eventIds)
            {
                Event _event = Database.GetEvent(id);
                if(_event.Id > -1)
                {
                    events.Add(_event);
                }
            }
                    
            nudge.Events = events.ToArray();
        }

        public static Nudge GetNudge(int id)
        {
            Nudge nudge = new Nudge();

            SqlCommand command = new SqlCommand("select * from nudges where id=@id;", _connection);

            command.Parameters.Add("id", System.Data.SqlDbType.Int);
            command.Parameters["id"].Value = id;

            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                nudge.Id = (int)reader["id"];
                nudge.Title = (string)reader["title"];
                nudge.Invitation = (string)reader["invitation"];
                nudge.Base64Cover = (string)reader["base64cover"];
                nudge.Base64Icon = (string)reader["base64icon"];
                nudge.Schedule = (DateTime)reader["schedule"];
            }
            reader.Close();

            if (nudge.Id > -1)
            {
                GetNudgeEvents(ref nudge);
            }
            return nudge;
        }
        #endregion


        #region USERS
        public static void AddUser(ref User user)
        {
            SqlCommand command = new SqlCommand("insert into users(eid,type,firstname,lastname) values(@eid,@type,@firstname,@lastname);SELECT CAST(scope_identity() AS int);", _connection);
            
            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters.Add("type", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("firstname", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("lastname", System.Data.SqlDbType.VarChar);
            
            command.Parameters["eid"].Value = user.Event.Id;
            command.Parameters["type"].Value = user.Type.EnumString();
            command.Parameters["firstname"].Value = user.FirstName;
            command.Parameters["lastname"].Value = user.LastName;

            object result = command.ExecuteScalar();
            if(result != null)
            {
                user.Id = Convert.ToInt32(result);
                user.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                user.DBStatus = DatabaseOpsStatus.ERROR;
            }

        }

        public static void UpdateUser(ref User user)
        {
            SqlCommand command = new SqlCommand("update users set firstname=@firstname,lastname=@lastname where id=@id AND eid=@eid;", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters.Add("firstname", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("lastname", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("id", System.Data.SqlDbType.Int);

            command.Parameters["eid"].Value = user.Event.Id;
            command.Parameters["firstname"].Value = user.FirstName;
            command.Parameters["lastname"].Value = user.LastName;
            command.Parameters["id"].Value = user.Id;

            int rows = command.ExecuteNonQuery();
            if(rows > 0)
            {
                user.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                user.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void DeleteUser(ref User user)
        {
            SqlCommand command = new SqlCommand("delete users where id=@id AND eid=@eid;", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters.Add("id", System.Data.SqlDbType.Int);

            command.Parameters["eid"].Value = user.Event.Id;
            command.Parameters["id"].Value = user.Id;

            int rows = command.ExecuteNonQuery();
            if(rows > 0)
            {
                user.DBStatus= DatabaseOpsStatus.SUCESS;
            }
            else
            {
                user.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void GetUsers(ref Event _event)
        {
            SqlCommand command = new SqlCommand("select * from users where eid=@eid;", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters["eid"].Value = _event.Id;

            List<User> attendees = new List<User>();
            SqlDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                string type = (string)reader["type"];
                int id = (int)reader["id"];
                string fname = (string)reader["firstname"];
                string lname = (string)reader["lastname"];

                if(type == User.UserType.OWNER.EnumString())
                {
                    _event.Owner = new User(id,_event, User.UserType.OWNER,fname,lname);
                }
                else if(type == User.UserType.MODERATOR.EnumString())
                {
                    _event.Moderator = new User(id, _event, User.UserType.MODERATOR, fname, lname);
                }
                else if(type == User.UserType.ATTENDEE.EnumString())
                {
                    attendees.Add(new User(id, _event, User.UserType.ATTENDEE, fname, lname));
                }
            }
            _event.Attendees = attendees.ToArray();
            reader.Close();
        }
        #endregion


        #region FILES
        public static void AddFile(ref Models.File file)
        {
            SqlCommand command = new SqlCommand("insert into files(eid,name,base64data) values(@eid,@name,@base64data);SELECT CAST(scope_identity() AS int);", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters.Add("name", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("base64data", System.Data.SqlDbType.Text);

            command.Parameters["eid"].Value = file.Event.Id;
            command.Parameters["name"].Value = file.Name;
            command.Parameters["base64data"].Value = file.Base64;

            object result = command.ExecuteScalar();
            if (result != null)
            {
                file.Id = Convert.ToInt32(result);
                file.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                file.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void UpdateFile(ref Models.File file)
        {
            SqlCommand command = new SqlCommand("update files set name=@name,base64data=@base64data where id=@id AND eid=@eid;", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters.Add("name", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("base64data", System.Data.SqlDbType.Text);
            command.Parameters.Add("id", System.Data.SqlDbType.Int);

            command.Parameters["eid"].Value = file.Event.Id;
            command.Parameters["name"].Value = file.Name;
            command.Parameters["base64data"].Value = file.Base64;
            command.Parameters["id"].Value = file.Id;

            int rows = command.ExecuteNonQuery();
            if(rows > 0)
            {
                file.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                file.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void DeleteFile(ref Models.File file)
        {
            SqlCommand command = new SqlCommand("delete files where id=@id AND eid=@eid;", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters.Add("id", System.Data.SqlDbType.Int);

            command.Parameters["eid"].Value = file.Event.Id;
            command.Parameters["id"].Value = file.Id;

            int rows = command.ExecuteNonQuery();
            if(rows > 0)
            {
                file.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                file.DBStatus = DatabaseOpsStatus.ERROR;
            }   
        }

        public static void GetFiles(ref Event _event)
        {
            SqlCommand command = new SqlCommand("select * from files where eid=@eid;", _connection);

            command.Parameters.Add("eid", System.Data.SqlDbType.Int);
            command.Parameters["eid"].Value = _event.Id;

            List<Models.File> files = new List<Models.File>();
            SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                int id = (int)reader["id"];
                string name = (string)reader["name"];
                string data = (string)reader["base64data"];

                files.Add(new Models.File(id, _event, name, data));
            }
            _event.Files = files.ToArray();
            reader.Close();
        }
        #endregion

        
        #region EVENTS
        public static void AddEvent(ref Models.Event _event)
        {
            SqlCommand command = new SqlCommand("insert into events(name,tagline,schedule,description,rigor_rank,category,sub_category) values(" +
                "@name,@tagline,@schedule,@description,@rigor_rank,@category,@sub_category);SELECT CAST(scope_identity() AS int);", _connection);

            command.Parameters.Add("name", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("tagline", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("schedule", System.Data.SqlDbType.DateTime);
            command.Parameters.Add("description", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("rigor_rank", System.Data.SqlDbType.Int);
            command.Parameters.Add("category", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("sub_category", System.Data.SqlDbType.VarChar);

            command.Parameters["name"].Value = _event.Name;
            command.Parameters["tagline"].Value = _event.TagLine;
            command.Parameters["schedule"].Value = _event.Schedule;
            command.Parameters["description"].Value = _event.Description;
            command.Parameters["rigor_rank"].Value = _event.Rigor_Rank;
            command.Parameters["category"].Value = _event.Category;
            command.Parameters["sub_category"].Value = _event.SubCategory;
           
            object result = command.ExecuteScalar();
            if (result != null)
            {
                _event.Id = Convert.ToInt32(result);
                if (_event.Id > -1)
                {
                    _event.DBStatus = DatabaseOpsStatus.SUCESS;

                    if (_event.Owner != null)
                    {
                        AddUser(ref _event.Owner);
                    }

                    if (_event.Moderator != null)
                    {
                        AddUser(ref _event.Moderator);
                    }

                    for (int i = 0; i < _event.Attendees.Length; i++)
                    {
                        AddUser(ref _event.Attendees[i]);
                    }

                    for (int i = 0; i < _event.Files.Length; i++)
                    {
                        AddFile(ref _event.Files[i]);
                    }
                }
                else
                {
                    _event.DBStatus = DatabaseOpsStatus.ERROR;
                }
            }
            else
            {
                _event.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }

        public static void UpdateEvent(ref Models.Event _event)
        {
            SqlCommand command = new SqlCommand("update events set name=@name,tagline=@tagline,schedule=@schedule,description=@description,rigor_rank=@rigor_rank,category=@category,sub_category=@sub_category where id =@id;", _connection);
            
            command.Parameters.Add("name", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("tagline", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("schedule", System.Data.SqlDbType.DateTime);
            command.Parameters.Add("description", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("rigor_rank", System.Data.SqlDbType.Int);
            command.Parameters.Add("category", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("sub_category", System.Data.SqlDbType.VarChar);
            command.Parameters.Add("id", System.Data.SqlDbType.Int);

            command.Parameters["name"].Value = _event.Name;
            command.Parameters["tagline"].Value = _event.TagLine;
            command.Parameters["schedule"].Value = _event.Schedule;
            command.Parameters["description"].Value = _event.Description;
            command.Parameters["rigor_rank"].Value = _event.Rigor_Rank;
            command.Parameters["category"].Value = _event.Category;
            command.Parameters["sub_category"].Value = _event.SubCategory;
            command.Parameters["id"].Value = _event.Id;

            command.ExecuteNonQuery();

            if(_event.Owner != null)
            {
                if(_event.Owner.Id <= -1)
                {
                    AddUser(ref _event.Owner);
                }
                else if(_event.Owner.Id > -1)
                {
                    if(_event.Owner.FirstName == "" &&  _event.Owner.LastName == "")
                    {
                        DeleteUser(ref _event.Owner);
                    }
                    else
                    {
                        UpdateUser(ref _event.Owner);
                    }
                }
            }

            if (_event.Moderator != null)
            {
                if (_event.Moderator.Id <= -1)
                {
                    AddUser(ref _event.Moderator);
                }
                else if (_event.Moderator.Id > -1)
                {
                    if (_event.Moderator.FirstName == "" && _event.Moderator.LastName == "")
                    {
                        DeleteUser(ref _event.Moderator);
                    }
                    else
                    {
                        UpdateUser(ref _event.Moderator);
                    }
                }
            }

            for(int i = 0; i < _event.Attendees.Length; i++)
            {
                if (_event.Attendees[i].Id <= -1)
                {
                    AddUser(ref _event.Attendees[i]);
                }
                else if (_event.Attendees[i].Id > -1)
                {
                    if (_event.Attendees[i].FirstName == "" && _event.Attendees[i].LastName == "")
                    {
                        DeleteUser(ref _event.Attendees[i]);
                    }
                    else
                    {
                        UpdateUser(ref _event.Attendees[i]);
                    }
                }
            }

            for (int i = 0; i < _event.Files.Length; i++)
            {
                if (_event.Files[i].Id <= -1)
                {
                    AddFile(ref _event.Files[i]);
                }
                else if (_event.Files[i].Id > -1)
                {
                    if (_event.Files[i].Name == "" && _event.Files[i].Base64 == "")
                    {
                        DeleteFile(ref _event.Files[i]);
                    }
                    else
                    {
                        UpdateFile(ref _event.Files[i]);
                    }
                }
            }
        }

        public static void DeleteEvent(ref Models.Event _event)
        {
            if (_event.Owner != null)
            {
                DeleteUser(ref _event.Owner);
            }

            if (_event.Moderator != null)
            {
                DeleteUser(ref _event.Moderator);
            }

            for(int i = 0;i < _event.Attendees.Length;i++)
            {
                DeleteUser(ref _event.Attendees[i]);
            }

            for(int i =0;i< _event.Files.Length;i++)
            {
                DeleteFile(ref _event.Files[i]);
            }

            SqlCommand command = new SqlCommand("delete events where id=@id;", _connection);

            command.Parameters.Add("id", System.Data.SqlDbType.Int);
            command.Parameters["id"].Value = _event.Id;

            int rows = command.ExecuteNonQuery();
            if (rows > 0)
            {
                _event.DBStatus = DatabaseOpsStatus.SUCESS;
            }
            else
            {
                _event.DBStatus = DatabaseOpsStatus.ERROR;
            }
        }
        
        public static Event GetEvent(int id)
        {
            Event _event = new Event();

            SqlCommand command = new SqlCommand("select * from events where id=@id;",_connection);

            command.Parameters.Add("id", System.Data.SqlDbType.Int);
            command.Parameters["id"].Value = id;

            SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                _event.Id = (int)reader["id"];
                _event.Name = (string)reader["name"];
                _event.TagLine = (string)reader["tagline"];
                _event.Schedule = (DateTime)reader["schedule"];
                _event.Description = (string)reader["description"];
                _event.Rigor_Rank = (int)reader["rigor_rank"];
                _event.Category = (string)reader["category"];
                _event.SubCategory = (string)reader["sub_category"];
            }
            reader.Close();

            if(_event.Id > -1)
            {
                GetUsers(ref _event);
                GetFiles(ref _event);
            }
            return _event;
        }

        public static List<Event> GetEvents(int limit, int page)
        {
            List<Models.Event> events = new List<Models.Event>();

            string countQueryString = "select count(*) from events where schedule >= GETDATE();";
            string mainQueryString = "select * from (select *,ROW_NUMBER() OVER(Order by Schedule) AS T from events where schedule >= GETDATE()) Q where T >= @start AND T < @end;";
                       
            SqlCommand commandI = new SqlCommand(countQueryString, _connection);
            object totalRows = commandI.ExecuteScalar();
            if(totalRows != null)
            {
                int rows = Convert.ToInt32(totalRows);
                if (rows > 0)
                {
                    int itemsPerPage = rows > limit ? rows / limit : limit;
                    int start = page * itemsPerPage;
                    int end = start + itemsPerPage;

                    SqlCommand command = new SqlCommand(mainQueryString, _connection);

                    command.Parameters.Add("start", System.Data.SqlDbType.Int);
                    command.Parameters.Add("end", System.Data.SqlDbType.Int);

                    command.Parameters["start"].Value = start;
                    command.Parameters["end"].Value = end;
                                        
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Event _event = new Event();
                        _event.Id = (int)reader["id"];
                        _event.Name = (string)reader["name"];
                        _event.TagLine = (string)reader["tagline"];
                        _event.Schedule = (DateTime)reader["schedule"];
                        _event.Description = (string)reader["description"];
                        _event.Rigor_Rank = (int)reader["rigor_rank"];
                        _event.Category = (string)reader["category"];
                        _event.SubCategory = (string)reader["sub_category"];
                        events.Add(_event); 
                    }
                    reader.Close();

                    for(int i =0;i < events.Count;i++)
                    {
                        if (events[i].Id > -1)
                        {
                            Event _event = events[i];
                            GetUsers(ref _event);
                            GetFiles(ref _event);
                            events[i] = _event;
                        }
                    }
                }
            }

            return events;
        }
        #endregion
    }
}
