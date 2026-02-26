using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GeniView.Cloud.Models
{
    public class UserActivityHistory
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        [Key]
        public long Id { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        [Required]
        [MaxLength(100)]
        public string UserFullName { get; set; }
        [MaxLength(100)]
        public string UserEmail { get; set; }
        public string Message { get; set; }
        public string AffectedObject { get; set; }

        public void AddActivity(string message, ActivityObjectType _type = ActivityObjectType.Message, string _object = null)
        {
            string result = "";
            switch (_type)
            {
                case ActivityObjectType.Community:
                    result = "Community: ";
                    break;
                case ActivityObjectType.Battery:
                    result = "Battery: ";
                    break;
                case ActivityObjectType.Device:
                    result = "Device: ";
                    break;
                case ActivityObjectType.Group:
                    result = "Group: ";
                    break;
                case ActivityObjectType.User:
                    result = "User: ";
                    break;
                case ActivityObjectType.Message:
                    break;
            }

            result += _object;

            using (var db = new IdentityDataRepository())
            {
                try
                {
                    // TODO Phase 4: pass currentUser explicitly from the controller (User property)
                    // GetCurrentUser() was removed; UserActivityHistory cannot resolve HttpContext from a model.
                    ApplicationUser currentUser = null;
                    var model = new UserActivityHistory()
                    {
                        Timestamp = DateTime.UtcNow,
                        UserFullName = currentUser != null ? currentUser.FullName : "No Info",
                        UserEmail = currentUser != null ? currentUser.Email : "No Info",
                        Message = message,
                        AffectedObject = result,
                    };

                    db.AddActivity(model);
                }
                catch (Exception ex)
                {
                    _logger.Error("Can not add User Activity", ex);
                }
            }
        }

        public void AddActivity(string message, ActivityObjectType _type, List<string> _objects)
        {
            if(_objects.Count == 1)
            {
                AddActivity(message, _type, _objects[0]);
                return;
            }
            string result="";
            switch(_type)
            {
                case ActivityObjectType.Community:
                    result = "Communities: ";
                    break;
                case ActivityObjectType.Battery:
                    result = "Batteries: ";
                    break;
                case ActivityObjectType.Device:
                    result = "Devices: ";
                    break;
                case ActivityObjectType.Group:
                    result = "Groups: ";
                    break;
                case ActivityObjectType.User:
                    result = "Users: ";
                    break;
            }

            result += String.Join(",", _objects);

            using (var db = new IdentityDataRepository())
            {
                try
                {
                    // TODO Phase 4: pass currentUser explicitly from the controller (User property)
                    ApplicationUser currentUser = null;
                    var model = new UserActivityHistory()
                    {
                        Timestamp = DateTime.UtcNow,
                        UserFullName = currentUser != null ? currentUser.FullName : "No Info",
                        UserEmail = currentUser != null ? currentUser.Email : "No Info",
                        Message = message,
                        AffectedObject = result,
                    };

                    db.AddActivity(model);
                }
                catch (Exception ex)
                {
                    _logger.Error("Can not add User Activity",ex);
                }
            }
        }
    }

    public class UserActivityHistoryFilter
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public List<UserActivityHistory> ActivityList { get; set; }
    }

    public enum ActivityObjectType
    {
        Community,
        Group,
        Battery,
        Device,
        User,
        Message
    }
}