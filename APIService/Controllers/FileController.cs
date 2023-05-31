using APIService.Components;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;

namespace APIService.Controllers
{
    [EnableCors("CORSPolicy")]
    [Route("api/v3/app/files")]
    public class FileController:Controller
    {
        [HttpGet()]
        public JsonResult GetFiles()
        {
            return new JsonResult(Database.GetFiles().JsonArray());
        }

        [HttpPost()]
        public JsonResult PostFiles()
        {
            string fileName = Request.Form.Files[0].FileName;
            var file = Request.Form.Files[0];
            var memoryStream = new MemoryStream();
            file.CopyToAsync(memoryStream);

            Models.File dbfile = new Models.File(-1, fileName, Convert.ToBase64String(memoryStream.ToArray()));
            Database.AddFile(ref dbfile);

            if (dbfile.Id > -1)
                return ServerStatus.PASS();
            else
                return ServerStatus.FAIL("database error");
        }

    }
}
