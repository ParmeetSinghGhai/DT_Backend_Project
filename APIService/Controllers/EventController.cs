using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Diagnostics;
using APIService.Components;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Cors;

namespace APIService.Controllers
{
    [EnableCors("CORSPolicy")]
    [Route("api/v3/app/events")]
    public class EventController : Controller
    {
        [HttpGet()]
        public JsonResult GetEvents()
        {
            RestStatus clientStatus = new RestStatus();
            object jsonData;
            string error;

            if (HttpContext.Request.Query.ContainsKey("id"))
            {
                int id = Convert.ToInt32(HttpContext.Request.Query["id"]);
                Models.Event _event = Database.GetEvent(id);
                if (_event.Id > -1)
                {
                    if (_event.Serialize(false, out jsonData, out error))
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
            else if(HttpContext.Request.Query.ContainsKey("type") 
                && HttpContext.Request.Query.ContainsKey("limit")
                && HttpContext.Request.Query.ContainsKey("page"))
            {
                int limit = Convert.ToInt32(HttpContext.Request.Query["limit"]);
                int page = Convert.ToInt32(HttpContext.Request.Query["page"]);

                List<Models.Event> events = Database.GetEvents(limit, page);
                clientStatus.json = new object[events.Count];
                for (int i =0; i < events.Count; i++)
                {
                    if(events[i].Serialize(false,out jsonData,out error))
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

            return Json(clientStatus);
        }

        [HttpPost()]
        public JsonResult AddEvent()
        {
            StreamReader streamreader = new StreamReader(Request.Body);
            string body = streamreader.ReadToEndAsync().Result;
            
            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Event newEvent = new Models.Event();
            bool status = newEvent.Deserialize(ref body, ref newEvent, out error);
            if (status)
            {
                Database.AddEvent(ref newEvent);
                if (newEvent.DBStatus == Database.DatabaseOpsStatus.SUCESS)
                {
                    object jsonData;
                    if(newEvent.Serialize(false,out jsonData, out error))
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

        [HttpPut("{id:int}")]
        public JsonResult UpdateEvent(int id)
        {
            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Event existingEvent = Database.GetEvent(id);
            if(existingEvent.Id > -1)
            {
                StreamReader reader = new StreamReader(HttpContext.Request.Body);
                string body = reader.ReadToEndAsync().Result;

                Models.Event _event = new Models.Event();
                _event.Id = id;

                bool status = _event.Deserialize(ref body, ref existingEvent, out error);
                if (status)
                {
                    Database.UpdateEvent(ref _event);
                    if (_event.DBStatus == Database.DatabaseOpsStatus.SUCESS)
                    {
                        _event = Database.GetEvent(_event.Id);
                        
                        object jsonData;
                        if (_event.Serialize(false, out jsonData, out error))
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
                        clientStatus.info = "no changes made to the event";//event did not update in database
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
                clientStatus.info = "event does not exist";//could not find event in database
            }

            return Json(clientStatus);
        }

        [HttpDelete("{id:int}")]
        public JsonResult DeleteEvent(int id)
        {
            string error = "";
            RestStatus clientStatus = new RestStatus();

            Models.Event _event = Database.GetEvent(id);
            if (_event.Id > -1)
            {               
                Database.DeleteEvent(ref _event);
                if (_event.DBStatus == Database.DatabaseOpsStatus.SUCESS)
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
                clientStatus.info = "event does not exist";//could not find event in database
            }

            return Json(clientStatus);
        }
    }
}