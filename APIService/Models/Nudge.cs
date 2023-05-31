using APIService.Components;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;
using static APIService.Models.User;

namespace APIService.Models
{
    public class Nudge
    {
        public int Id;
        public string Title;
        public string Invitation;
        public string Base64Cover;
        public string Base64Icon;
        public DateTime Schedule;
        public Event[] Events;

        public Database.DatabaseOpsStatus DBStatus;

        public Nudge()
        {
            Id = -1;
            Events = new Event[0];
            Title = "";
            Base64Cover = "";
            Base64Icon = "";
            Schedule = DateTime.Now;
        }
        
        public bool Deserialize(ref string jsonInput, ref Nudge mirrorNudge, out string error)
        {
            try
            {
                JsonElement jsonPayload = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                JsonElement temp = new JsonElement();
               
                if (jsonPayload.TryGetProperty("title", out temp))
                {
                    this.Title = temp.GetSafeString();
                }
                else
                {
                    this.Title = mirrorNudge.Title;
                }

                if (jsonPayload.TryGetProperty("invitation", out temp))
                {
                    this.Invitation = temp.GetSafeString();
                }
                else
                {
                    this.Invitation = mirrorNudge.Invitation;
                }

                if (jsonPayload.TryGetProperty("base64cover", out temp))
                {
                    this.Base64Cover = temp.GetSafeString();
                }
                else
                {
                    this.Base64Cover = mirrorNudge.Base64Cover;
                }

                if (jsonPayload.TryGetProperty("base64icon", out temp))
                {
                    this.Base64Icon = temp.GetSafeString();
                }
                else
                {
                    this.Base64Icon = mirrorNudge.Base64Icon;
                }

                if (jsonPayload.TryGetProperty("schedule", out temp))
                {
                    DateTime datetime;
                    if (DateTime.TryParse(temp.GetSafeString(), out datetime))
                        this.Schedule = datetime;
                }
                else
                {
                    this.Schedule = mirrorNudge.Schedule;
                }

                if (jsonPayload.TryGetProperty("events", out temp))
                {
                    List<Event> events = new List<Event>();
                    for (int i = 0; i < temp.GetArrayLength(); i++)
                    {
                        int eventId;
                        if(temp[i].TryGetInt32(out eventId))
                        {
                            int absEventId = Math.Abs(eventId);
                            Event _event = Database.GetEvent(absEventId);
                            if(_event.Id > -1)
                            {
                                _event.Id = eventId;
                                events.Add(_event);
                            }
                        }
                    }
                    Events = events.ToArray();
                }

                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message + "." + ex.StackTrace;
                return false;
            }
        }

        public bool Serialize(bool minimal, out object jsonObject, out string error)
        {
            error = "";
            try
            {
                Dictionary<string, object> json = new Dictionary<string, object>();
                json["id"] = Id;
                json["title"] = Title;
                json["invitation"] = Invitation;
                json["base64cover"] = Base64Cover;
                json["base64icon"] = Base64Icon;
                json["schedule"] = Schedule;
             
                if (minimal)
                {
                    json["events"] = new int[Events.Length];
                    for (int i = 0; i < Events.Length; i++)
                    {
                        ((int[])json["files"])[i] = Events[i].Id;
                    }
                }
                else
                {
                    json["events"] = new object[Events.Length];
                    for (int i = 0; i < Events.Length; i++)
                    {
                        object eventData;
                        if(Events[i].Serialize(false, out eventData, out error))
                        {
                            ((object[])json["events"])[i] = eventData;
                        }
                    }
                }

                error = "";
                jsonObject = json;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message + "." + ex.StackTrace;
                jsonObject = null;
                return false;
            }
        }


    }

 
}
