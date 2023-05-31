using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace APIService.Components
{
    public static class Extensions
    {
        public static string GetSafeString(this JsonElement element)
        {
            if (element.GetString() != null)
                return element.GetString();
            return "";
        }

        public static int GetSafeInt(this JsonElement element)
        {
            int value = -1;
            element.TryGetInt32(out value);
            return value;
        }

        public static bool TryGetArrayData(this JsonElement array, out string[] data, out int id)
        {
            data = new string[2];
            id = -1;

            if (array[0].GetInt32() > -1)
            {
                /*
                 * if the user or file already exists in the database then the user or file will only be added in 
                 * the event object when either its firstname and lastname / name and base64data are both NOT NULL
                 * in which case the user or file will be updated or when its firstname and lastname / name and 
                 * base64data are both NULL in which case the user or file will be deleted from the database
                 * for that event
                 */
                id = array[0].GetInt32();
                if (array[1].GetString() != null && array[2].GetString() != null)
                {
                    data[0] = array[1].GetString();
                    data[1] = array[2].GetString();
                    return true;
                }
                else if (array[1].GetString() == null && array[2].GetString() == null)
                {
                    data[0] = "";
                    data[1] = "";
                    return true;
                }
            }
            else
            {
                /*
                 * if the user or file does not exist in the database then both firstname and lastname /name and 
                 * base64data are needed to be NOT NULL in order for the user to be added in the event regardless
                 * of what the role is (owner,moderator or attendee) or file type is
                 */
                if (array[1].GetString() != null && array[2].GetString() != null)
                {
                    data[0] = array[1].GetString();
                    data[1] = array[2].GetString();
                    return true;
                }
            }
            return false;
        }
        
        public static bool TryGetUserName(this JsonElement element,string propertyName,out string[] name, out int id)
        {
            name = new string[2];
            id = -1;
            
            JsonElement array;
            if(element.TryGetProperty(propertyName, out array))
            {
                return array.TryGetArrayData(out name, out id);
            }
            return false;
        }
                
        public static string EnumString(this RestStatus.StatusCode status)
        {
            if (status == RestStatus.StatusCode.Success)
                return "success";
            else
                return "error";
        }

        public static string EnumString(this Models.User.UserType type)
        {
            if (type == Models.User.UserType.OWNER)
                return "owner";
            else if (type == Models.User.UserType.MODERATOR)
                return "moderator";
            else if (type == Models.User.UserType.ATTENDEE)
                return "attendee";
            else
                return "";

        }
    }
}
