using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace APIService.Components
{
    public class RestStatus
    {
        public enum StatusCode 
        { 
            Success, Error 
        }

        public string status { get; set; }
        public object json { get; set; }
        public string info { get; set; }
               
        public RestStatus()
        {
            status = StatusCode.Success.EnumString();
            json = "";
            info = "";
        }
    }
}
