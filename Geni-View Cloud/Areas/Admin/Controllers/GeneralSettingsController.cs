using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Application Admin")]
    public class GeneralSettingsController : Controller
    {
        public ActionResult Index()
        {
            GlobalSettingModel model = new GlobalSettingModel()
            {
                // Colors
                SuccessColor = GlobalSettings.SuccessColor,
                AlertColor = GlobalSettings.AlertColor,
                WarningColor = GlobalSettings.WarningColor,
                // Charge Level
                SuccessChargingLVL = GlobalSettings.SuccessChargingLVL,
                AlertChargingLVL = GlobalSettings.AlertChargingLVL,
                IsStateOfChargeReadyToUse = GlobalSettings.IsStateOfChargeReadyToUse,
                //Temperature
                AlertTemperature = GlobalSettings.AlertTemperature,
                SuccessTemperature = GlobalSettings.SuccessTemperature,
                // Time Ranges
                OnlineRangeInMinutes = GlobalSettings.OnlineRangeInMinutesValue,
                OfflineRangeInDays = GlobalSettings.OfflineRangeInDaysValue,
                // Other Settings
                NominalVoltage = GlobalSettings.NominalVoltage,
                // Map Key
                BingMapKey = GlobalSettings.BingMapKey,

                NotificationDelayTimeInSeconds = GlobalSettings.NotificationDelayTimeInSeconds,
                NotificationToleranceInMinutes = GlobalSettings.NotificationToleranceInMinutes,

                UserLockoutTimeInMinutes = GlobalSettings.UserLockoutTimeInMinutes,
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(GlobalSettingModel model)
        {
            TempData["Success"] = "All settings successfully saved.";
            GlobalSettings.SuccessColor = model.SuccessColor;
            GlobalSettings.AlertColor = model.AlertColor;
            GlobalSettings.WarningColor = model.WarningColor;

            GlobalSettings.OfflineRangeInDaysValue = model.OfflineRangeInDays;
            GlobalSettings.OnlineRangeInMinutesValue = model.OnlineRangeInMinutes;

            GlobalSettings.NominalVoltage = model.NominalVoltage != null ? model.NominalVoltage : 21.6;

            GlobalSettings.SuccessChargingLVL = model.SuccessChargingLVL;
            GlobalSettings.AlertChargingLVL = model.AlertChargingLVL;
            GlobalSettings.IsStateOfChargeReadyToUse = model.IsStateOfChargeReadyToUse;

            GlobalSettings.AlertTemperature = model.AlertTemperature;
            GlobalSettings.SuccessTemperature = model.SuccessTemperature;

            GlobalSettings.BingMapKey = model.BingMapKey;

            GlobalSettings.NotificationDelayTimeInSeconds = model.NotificationDelayTimeInSeconds;
            GlobalSettings.NotificationToleranceInMinutes = model.NotificationToleranceInMinutes;

            GlobalSettings.UserLockoutTimeInMinutes = model.UserLockoutTimeInMinutes;

            return View(model);
        }
    }
}