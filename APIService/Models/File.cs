using APIService.Components;
using Microsoft.AspNetCore.Mvc;

namespace APIService.Models
{
    public class File
    {
        public int Id;
        public string Name;
        public string Base64;
        public Event Event;
        public Database.DatabaseOpsStatus DBStatus;

        public File(int id, Event _event, string name, string base64)
        {
            Id = id;
            Event = _event;
            Name = name;
            Base64 = base64;
            DBStatus = Database.DatabaseOpsStatus.SUCESS;
        }

        public File(int id, Event _event)
        {
            Id = id;
            Event = _event;
        }

        private File()
        {
            //default constructor disabled
        }
    }
}
