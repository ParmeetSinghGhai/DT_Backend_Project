using APIService.Components;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace APIService.Controllers
{
    [EnableCors("CORSPolicy")]
    [Route("api/v3/app/users")]
    public class UserController:Controller
    {
        [HttpGet()]
        public JsonResult GetUsers()
        {
            return new JsonResult(Database.GetUsers().JsonArray());
        }

        [HttpPost()]
        public JsonResult PostUsers()
        {
            StreamReader r = new StreamReader(Request.Body);
            string body = r.ReadToEndAsync().Result;
            Models.User newUser = JsonSerializer.Deserialize<Models.User>(body);
            Database.AddUser(ref newUser);
            if(newUser.Id > -1)
            {
                return ServerStatus.PASS();
            }
            else
            {
                return ServerStatus.FAIL("database error");
            }
        }

    }
}
