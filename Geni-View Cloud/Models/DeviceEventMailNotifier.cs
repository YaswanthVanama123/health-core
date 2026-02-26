using GeniView.Cloud.Repository;
using GeniView.Data.Hardware;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeniView.Cloud.Models
{
    public class DeviceEventMailNotifier
    {
        private readonly MailHelper mailHelper;
        private List<UserViewModel>? users;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        // TODO Phase 8: inject IHubContext<NotificationHub> via constructor for SignalR push notifications.
        // In Program.cs register: builder.Services.AddSignalR(); app.MapHub<NotificationHub>("/notificationHub");
        // Then inject: private readonly IHubContext<NotificationHub> _hubContext;

        public DeviceEventMailNotifier()
        {
            this.mailHelper = new MailHelper();
        }

        public async Task SendMessageAsync(IEnumerable<DeviceEvent> deviceEvents)
        {
            foreach (var item in deviceEvents)
            {
                await SendMessageAsync(item);
            }
        }

        public async Task SendMessageAsync(DeviceEvent deviceEvent)
        {
            if (!CheckEventRules(deviceEvent))
                return;

            Device? originDevice;
            bool isMessageSent = false;
            try
            {
                using (var deviceDb = new DevicesDataRepository())
                {
                    originDevice = deviceDb.FindBySN(deviceEvent.DeviceSerialNumber);
                }

                if (originDevice != null && originDevice.IsDeactivated == false)
                {
                    long? nullableLong = null;
                    using (var userDb = new IdentityDataRepository())
                    {
                        this.users = userDb.GetUsersWhoHasAccess(
                            originDevice.Community != null ? originDevice.Community.ID : nullableLong,
                            originDevice.Group     != null ? originDevice.Group.ID     : nullableLong
                        ).ToList();
                    }

                    foreach (var user in users)
                    {
                        if (user.User.Email == "admin@bytec.com")
                            continue;

                        if (!user.User.IsNotificationEnable)
                            continue;

                        try
                        {
                            // TODO Phase 8: replace with IHubContext<NotificationHub> push
                            // await _hubContext.Clients.User(user.User.Email).SendAsync("addNotification", ...);

                            await mailHelper.SendMailAsync(user.User.Email!, deviceEvent);
                            isMessageSent = true;
                            await Task.Delay(GlobalSettings.NotificationDelayTimeInSeconds * 1000);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("DeviceEventMailNotifier error.", ex);
                        }
                    }

                    if (isMessageSent)
                    {
                        using var db = new GeniViewCloudDataRepository();
                        try
                        {
                            DateTime oldestNotifiableEventDate = DateTime.UtcNow.AddMinutes(GlobalSettings.NotificationToleranceInMinutes * -1);
                            DeviceEvent? originEvent = db.DeviceEvents
                                                        .AsEnumerable()
                                                        .Where(d => d.Timestamp == deviceEvent.Timestamp
                                                                 && d.DeviceSerialNumber == deviceEvent.DeviceSerialNumber
                                                                 && d.Timestamp > oldestNotifiableEventDate)
                                                        .FirstOrDefault();
                            if (originEvent != null)
                            {
                                originEvent.IsHandled = true;
                                db.Entry(originEvent).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("DeviceEventMailNotifier save error.", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("DeviceEventMailNotifier error.", ex);
            }
        }

        private bool CheckEventRules(DeviceEvent deviceEvent)
        {
            DateTime oldestNotifiableEventDate = DateTime.UtcNow.AddMinutes(GlobalSettings.NotificationToleranceInMinutes * -1);

            if (deviceEvent.Timestamp < oldestNotifiableEventDate)
                return false;

                using var db = new GeniViewCloudDataRepository();
            return !db.DeviceEvents.Where(x => x.UID == deviceEvent.UID
                                            && x.DeviceSerialNumber == deviceEvent.DeviceSerialNumber
                                            && x.Timestamp > oldestNotifiableEventDate)
                                   .Any(x => x.IsHandled == true);
        }
    }
}
