using APIService.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace APIService.Models
{
    public class Event
    {
        public int Id;
        public string Type;
        public string Name;
        public string TagLine;
        public DateTime Schedule;
        public string Description;
        public string Category;
        public string SubCategory;
        public int Rigor_Rank;

        public User Owner;//replacement for uid
        public User Moderator;
        public User[] Attendees;

        public File[] Files;
       
        public Nudge[] Nudges;

        public Database.DatabaseOpsStatus DBStatus;

        public Event()
        {
            Id = -1;
            Type = "event";
          
            Name = "";
            TagLine = "";
            Schedule = DateTime.Today;
            Description = "";
            Category = "";
            SubCategory = "";
            Rigor_Rank = -1;
                        
            Owner = null;
            Moderator = null;
            Attendees = new User[0];

            Files = new File[0];

            Nudges = new Nudge[0];

            DBStatus = Database.DatabaseOpsStatus.SUCESS;
        }

        public bool Deserialize(ref string jsonInput,ref Event mirrorEvent, out string error)
        {
            try
            {
                JsonElement jsonPayload = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                JsonElement temp = new JsonElement();
                string[] name;
                int id;

                if (jsonPayload.TryGetProperty("name", out temp))
                {
                    this.Name = temp.GetSafeString();
                }
                else
                {
                    this.Name = mirrorEvent.Name;
                }

                if (jsonPayload.TryGetProperty("tagline", out temp))
                {
                    this.TagLine = temp.GetSafeString();
                }
                else
                {
                    this.TagLine = mirrorEvent.TagLine;
                }

                if (jsonPayload.TryGetProperty("description", out temp))
                {
                    this.Description = temp.GetSafeString();
                }
                else
                {
                    this.Description = mirrorEvent.Description;
                }

                if (jsonPayload.TryGetProperty("category", out temp))
                {
                    this.Category = temp.GetSafeString();
                }
                else
                {
                    this.Category = mirrorEvent.Category;
                }

                if (jsonPayload.TryGetProperty("subcategory", out temp))
                {
                    this.SubCategory = temp.GetSafeString();
                }
                else
                {
                    this.SubCategory = mirrorEvent.SubCategory;
                }

                if (jsonPayload.TryGetProperty("schedule",out temp))
                {
                    DateTime datetime;
                    if(DateTime.TryParse(temp.GetSafeString(), out datetime))
                        this.Schedule = datetime;
                }   
                else
                {
                    this.Schedule = mirrorEvent.Schedule;
                }

                if (jsonPayload.TryGetProperty("rigor_rank",out temp))
                {
                    this.Rigor_Rank = temp.GetSafeInt();
                }
                else
                {
                    this.Rigor_Rank = mirrorEvent.Rigor_Rank;
                }


                if (jsonPayload.TryGetUserName("owner",out name,out id))
                {
                    this.Owner = new Models.User(id, this, User.UserType.OWNER, name[0], name[1]);
                }
                if (jsonPayload.TryGetUserName("moderator", out name, out id))
                {
                    this.Moderator = new Models.User(id, this, User.UserType.MODERATOR, name[0], name[1]);
                }
                if (jsonPayload.TryGetProperty("attendees", out temp))
                {
                    List<User> attendees = new List<User>();
                    for(int i = 0;i < temp.GetArrayLength(); i++)
                    {
                        if (temp[i].TryGetArrayData(out name,out id))
                            attendees.Add(new Models.User(id, this, User.UserType.ATTENDEE, name[0], name[1]));
                    }
                    this.Attendees = attendees.ToArray();
                }


                if(jsonPayload.TryGetProperty("files",out temp))
                {
                    List<File> files = new List<File>();
                    for (int i = 0; i < temp.GetArrayLength(); i++)
                    {
                        if (temp[i].TryGetArrayData(out name,out id))
                        {
                            files.Add(new Models.File(id, this, name[0], name[1]));
                        }
                    }
                    this.Files = files.ToArray();
                }
                            
                error = "";
                return true;
            }
            catch(Exception ex)
            {
                error = ex.Message + "."+ ex.StackTrace;
                return false;
            }
        }

        public bool Serialize(bool minimal,out object jsonObject, out string error)
        {
            error = "";
            try
            {
                Dictionary<string, object> json = new Dictionary<string, object>();
                json["id"] = Id;
                json["name"] = Name;
                json["tagline"] = TagLine;
                json["schedule"] = Schedule;
                json["description"] = Description;
                json["category"] = Category;
                json["subcategory"] = SubCategory;
                json["rigor_rank"] = Rigor_Rank;

                if (minimal)
                    json["owner"] = Owner == null ? -1 : Owner.Id;
                else
                    json["owner"] = Owner == null ? new string[3] { "-1", "", "" } : new string[3] { Owner.Id.ToString(), Owner.FirstName, Owner.LastName };

                if(minimal)
                    json["moderator"] = Moderator == null ? -1 : Moderator.Id;
                else
                    json["moderator"] = Moderator == null ? new string[3] { "-1", "", "" } : new string[3] { Moderator.Id.ToString(), Moderator.FirstName, Moderator.LastName };

                if (minimal)
                {
                    json["attendees"] = new int[Attendees.Length];
                    for (int i = 0; i < Attendees.Length; i++)
                    {
                        ((int[])json["attendees"])[i] = Attendees[i].Id;
                    }
                }
                else
                {
                    json["attendees"] = new string[Attendees.Length][];
                    for (int i = 0; i < Attendees.Length; i++)
                    {
                        ((string[][])json["attendees"])[i] = new string[3];
                        ((string[][])json["attendees"])[i][0] = Attendees[i].Id.ToString();
                        ((string[][])json["attendees"])[i][1] = Attendees[i].FirstName;
                        ((string[][])json["attendees"])[i][2] = Attendees[i].LastName;
                    }
                }
                
                if(minimal)
                {
                    json["files"] = new int[Files.Length];
                    for (int i = 0; i < Files.Length; i++)
                    {
                        ((int[])json["files"])[i] = Files[i].Id;
                    }
                }
                else
                {
                    json["files"] = new string[Files.Length][];
                    for (int i = 0; i < Files.Length; i++)
                    {
                        ((string[][])json["files"])[i] = new string[3];
                        ((string[][])json["files"])[i][0] = Files[i].Id.ToString();
                        ((string[][])json["files"])[i][1] = Files[i].Name;
                        ((string[][])json["files"])[i][2] = Files[i].Base64;
                    }
                }
                
                error = "";
                jsonObject = json;
                return true;
            }
            catch(Exception ex)
            {
                error = ex.Message + "." + ex.StackTrace;
                jsonObject = null;
                return false;
            }
        }


       

    }

   
}
