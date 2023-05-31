using APIService.Components;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static APIService.Models.User;

namespace APIService.Models
{
    public class User
    {
        public enum UserType
        {
            OWNER,
            MODERATOR,
            ATTENDEE
        }

        public int Id;
        public string FirstName;
        public string LastName;
        public Event Event;
        public UserType Type;
        public Database.DatabaseOpsStatus DBStatus;
        
        public User(int id, Event _event, UserType type, string firstName, string lastName)
        {
            Id = id;
            Event = _event;
            FirstName = firstName;
            LastName = lastName;
            Type = type;
            DBStatus = Database.DatabaseOpsStatus.SUCESS;
        }
        
        public User(int id, Event _event)
        {
            Id = id;
            Event = _event;
        }

        private User()
        {
            //default constructor disabled
        }
    }
}
