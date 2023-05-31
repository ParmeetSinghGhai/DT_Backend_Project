using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;
using APIService.Components;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Cors;
using APIService.Models;
using Microsoft.Extensions.Logging;

namespace APIService.Controllers
{
    [EnableCors("CORSPolicy")]
    [Route("api/v3/app/nudges")]
    public class NudgeController:Controller
    {
        [HttpGet()]
        public JsonResult GetNudges()
        {
            RestStatus clientStatus = new RestStatus();
            object jsonData;
            string error;

            if (HttpContext.Request.Query.ContainsKey("id"))
            {
                int id = Convert.ToInt32(HttpContext.Request.Query["id"]);
                Models.Nudge nudge = Database.GetNudge(id);
                if (nudge.Id > -1)
                {
                    if (nudge.Serialize(false, out jsonData, out error))
                    {
                        clientStatus.status = RestStatus.StatusCode.Success.EnumString();
                        clientStatus.json = jsonData;
                        clientStatus.info = "";
                    }
                    else
                    {
                        clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                        clientStatus.json = null;
                        clientStatus.info = error;
                    }
                }
                else
                {
                    clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                    clientStatus.json = null;
                    clientStatus.info = "event does not exist";
                }
            }
            else if(HttpContext.Request.Query.ContainsKey("eventid"))
            {
                int id = Convert.ToInt32(HttpContext.Request.Query["eventid"]);
                Event _event = Database.GetEvent(id);
                if(_event.Id > -1)
                {
                    List<Nudge> nudges = Database.GetEventNudges(ref _event);
                    clientStatus.json = new object[nudges.Count];
                    for (int i = 0; i < nudges.Count; i++)
                    {
                        if (nudges[i].Serialize(false, out jsonData, out error))
                        {
                            clientStatus.status = RestStatus.StatusCode.Success.EnumString();
                            ((object[])clientStatus.json)[i] = jsonData;
                            clientStatus.info = "";
                        }
                        else
                        {
                            clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                            clientStatus.json = null;
                            clientStatus.info = error;
                            break;
                        }
                    }
                }
                else
                {
                    clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                    clientStatus.json = null;
                    clientStatus.info = "event does not exist";
                }
            }

            return Json(clientStatus);
        }

        [HttpPost()]
        public JsonResult AddNudge()
        {
            StreamReader streamreader = new StreamReader(Request.Body);
            string body = streamreader.ReadToEndAsync().Result;

            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Nudge newNudge = new Models.Nudge();
            bool status = newNudge.Deserialize(ref body, ref newNudge, out error);
            if (status)
            {
                Database.AddNudge(ref newNudge);
                if (newNudge.DBStatus == Database.DatabaseOpsStatus.SUCESS)
                {
                    object jsonData;
                    if (newNudge.Serialize(false, out jsonData, out error))
                    {
                        clientStatus.status = RestStatus.StatusCode.Success.EnumString();
                        clientStatus.json = jsonData;
                        clientStatus.info = "";
                    }
                    else
                    {
                        clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                        clientStatus.json = null;
                        clientStatus.info = error;
                    }
                }
                else
                {
                    clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                    clientStatus.json = null;
                    clientStatus.info = "unknown error";
                }
            }
            else
            {
                clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                clientStatus.json = null;
                clientStatus.info = error;
            }

            return Json(clientStatus);
        }

        [HttpPost("{id:int}")]
        public JsonResult CloneNudge(int id)
        {
            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Nudge existingNudge = Database.GetNudge(id);
            if (existingNudge.Id > -1)
            {
                Models.Nudge nudge = new Models.Nudge();
                nudge.Title = existingNudge.Title;
                nudge.Invitation = existingNudge.Invitation;
                nudge.Base64Cover = existingNudge.Base64Cover;
                nudge.Base64Icon = existingNudge.Base64Icon;
                nudge.Schedule = existingNudge.Schedule;
                nudge.Events = existingNudge.Events;

                Database.AddNudge(ref nudge);
                if (nudge.DBStatus == Database.DatabaseOpsStatus.SUCESS)
                {
                    object jsonData;
                    if (nudge.Serialize(false, out jsonData, out error))
                    {
                        clientStatus.status = RestStatus.StatusCode.Success.EnumString();
                        clientStatus.json = jsonData;
                        clientStatus.info = "";//success
                    }
                    else
                    {
                        clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                        clientStatus.json = null;
                        clientStatus.info = error;//event data could not be serialized(mode:minified)
                    }
                }
                else
                {
                    clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                    clientStatus.json = null;
                    clientStatus.info = "clone of nudge could not be created";//event did not update in database
                }
            }
            else
            {
                clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                clientStatus.json = null;
                clientStatus.info = "nudge does not exist";//could not find event in database
            }

            return Json(clientStatus);
        }

        [HttpPut("{id:int}")]
        public JsonResult UpdateNudge(int id)
        {
            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Nudge existingNudge = Database.GetNudge(id);
            if (existingNudge.Id > -1)
            {
                StreamReader reader = new StreamReader(HttpContext.Request.Body);
                string body = reader.ReadToEndAsync().Result;

                Models.Nudge nudge = new Models.Nudge();
                nudge.Id = id;

                bool status = nudge.Deserialize(ref body, ref existingNudge, out error);
                if (status)
                {
                    Database.UpdateNudge(ref nudge);
                    if (nudge.DBStatus == Database.DatabaseOpsStatus.SUCESS)
                    {
                        nudge = Database.GetNudge(nudge.Id);

                        object jsonData;
                        if (nudge.Serialize(false, out jsonData, out error))
                        {
                            clientStatus.status = RestStatus.StatusCode.Success.EnumString();
                            clientStatus.json = jsonData;
                            clientStatus.info = "";//success
                        }
                        else
                        {
                            clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                            clientStatus.json = null;
                            clientStatus.info = error;//event data could not be serialized(mode:minified)
                        }
                    }
                    else
                    {
                        clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                        clientStatus.json = null;
                        clientStatus.info = "no changes made to the nudge";//event did not update in database
                    }
                }
                else
                {
                    clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                    clientStatus.json = null;
                    clientStatus.info = error;//request body not getting deserialized
                }
            }
            else
            {
                clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                clientStatus.json = null;
                clientStatus.info = "nudge does not exist";//could not find event in database
            }

            return Json(clientStatus);
        }


        [HttpDelete("{id:int}")]
        public JsonResult DeleteNudge(int id)
        {
            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Nudge nudge = Database.GetNudge(id);
            if (nudge.Id > -1)
            {
                Database.DeleteNudge(ref nudge);
                if (nudge.DBStatus == Database.DatabaseOpsStatus.SUCESS)
                {
                    clientStatus.status = RestStatus.StatusCode.Success.EnumString();
                    clientStatus.json = "";
                    clientStatus.info = "";//success
                }
                else
                {
                    clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                    clientStatus.json = null;
                    clientStatus.info = error;//error deleting event in database
                }
            }
            else
            {
                clientStatus.status = RestStatus.StatusCode.Error.EnumString();
                clientStatus.json = null;
                clientStatus.info = "nudge does not exist";//could not find event in database
            }

            return Json(clientStatus);
        }
    }
}
