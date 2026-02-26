using GeniView.Cloud.Common;
using GeniView.Cloud.Repository;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GeniView.Cloud.Models
{
    public class MailHelper
    {
        private MailServer? model;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public MailHelper()
        {
            using (var db = new GeniViewCloudDataRepository())
            {
                model = db.MailServer.FirstOrDefault();
            }
        }

        public async Task SendMailAsync(string destination, string subject, string body)
        {
            if (model != null)
            {
                using var client = new SmtpClient
                {
                    Host = model.Host,
                    Port = model.Port,
                    DeliveryMethod = model.DeliveryMethod,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(model.User, model.Password),
                    EnableSsl = model.EnableSsl,
                };

                var from = new MailAddress(model.User);
                var to   = new MailAddress(destination);

                // TODO Phase 5: replace ViewRenderer.RenderView with ViewRenderService
                var mail = new MailMessage(from, to)
                {
                    Subject     = subject,
                    Body        = body,
                    IsBodyHtml  = true,
                    ReplyTo     = model.ReplyTo != null ? new MailAddress(model.ReplyTo) : from,
                };

                try { client.Send(mail); }
                catch (Exception ex)
                {
                    _logger.Error("MailHelper SendMailAsync error.", ex);
                }
            }
        }

        public async Task SendMailAsync(string fullName, string email, MessageEnumeration mEnum, string callbackUrl)
        {
            if (model != null)
            {
                using var client = new SmtpClient
                {
                    Host = model.Host,
                    Port = model.Port,
                    DeliveryMethod = model.DeliveryMethod,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(model.User, model.Password),
                    EnableSsl = model.EnableSsl,
                };

                var from = new MailAddress(model.User);
                var to   = new MailAddress(email);

                string subject = mEnum switch
                {
                    MessageEnumeration.ResetPassword  => "Geni-View Cloud Reset Password Request",
                    MessageEnumeration.ConfirmEmail   => "Geni-View Cloud Email Confirmation",
                    _                                 => "Geni-View Cloud Mail Server Configuration",
                };

                // TODO Phase 5: render via ViewRenderService instead of string concatenation
                string body = $"Hello {fullName}, please click: {callbackUrl}";

                var mail = new MailMessage(from, to)
                {
                    Subject    = subject,
                    Body       = body,
                    IsBodyHtml = true,
                    ReplyTo    = model.ReplyTo != null ? new MailAddress(model.ReplyTo) : from,
                };

                try { client.Send(mail); }
                catch (Exception ex)
                {
                    _logger.Error("MailHelper SendMailAsync error.", ex);
                    throw;
                }
            }
        }

        public async Task SendMailAsync(string destination, DeviceEvent deviceEvent)
        {
            if (model != null)
            {
                using var client = new SmtpClient
                {
                    Host = model.Host,
                    Port = model.Port,
                    DeliveryMethod = model.DeliveryMethod,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(model.User, model.Password),
                    EnableSsl = model.EnableSsl,
                };

                var from = new MailAddress(model.User);
                var to   = new MailAddress(destination);
                try
                {
                    // HostingEnvironment.MapPath replaced with Global._serverPath (set in Program.cs)
                    string templatePath = Path.Combine(Global._serverPath, "Views", "MessageBodies", "DeviceEvent.html");
                    string mBody = File.ReadAllText(templatePath);
                    mBody = mBody.Replace("#mSubject",            "Geni - View Cloud Device Notification");
                    mBody = mBody.Replace("#mDeviceSerialNumber", deviceEvent.DeviceSerialNumber);
                    mBody = mBody.Replace("#mEventType",          deviceEvent.EventTypeText);
                    mBody = mBody.Replace("#mDescription",        deviceEvent.Description);
                    mBody = mBody.Replace("#mSource",             deviceEvent.SourceText);
                    mBody = mBody.Replace("#mTimestamp",          deviceEvent.Timestamp.ToString());
                    mBody = mBody.Replace("#mDateTimeNow",        DateTime.UtcNow.Year.ToString());

                    var mail = new MailMessage(from, to)
                    {
                        Subject    = "Geni-View Cloud Device Notification",
                        Body       = mBody,
                        IsBodyHtml = true,
                        ReplyTo    = model.ReplyTo != null ? new MailAddress(model.ReplyTo) : from,
                    };
                    client.Send(mail);
                }
                catch (Exception ex)
                {
                    _logger.Error("MailHelper SendMailAsync error.", ex);
                }
            }
            else
            {
                _logger.Warn("Cannot send notifications, because mail server configuration not completed.");
            }
        }

        public bool IsMailServerConfigured() => model != null;
    }
}
