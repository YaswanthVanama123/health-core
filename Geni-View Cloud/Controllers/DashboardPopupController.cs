using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Controllers
{
    [Authorize]
    public sealed class DashboardPopupController : Controller
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private dynamic /* TODO Phase 4: ApplicationUserManager */ UserManager
        {
            get { throw new NotImplementedException("TODO Phase 4: inject UserManager via ASP.NET Core Identity"); }
        }

        [HttpGet]
        public JsonResult GetSocPopupData(string cardKey, string search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUser = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));

                long? communityId = null;
                long? groupId = null;
                var includeAllSubGroups = SessionHelper.IncludeAllSubGroups ?? true;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    communityId = SessionHelper.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID;
                }
                else if (User.IsInRole("Community User"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID ?? SessionHelper.GroupID;
                }

                using (var repo = new DashboardDataRepository())
                using (var popupRepo = new DashboardPopupRepository())
                {
                    var soc = repo.GetStateOfCharge(communityId, groupId, includeAllSubGroups);

                    IEnumerable<long> ids = Enumerable.Empty<long>();
                    var key = (cardKey ?? string.Empty).Trim().ToLowerInvariant();

                    if (key == "high") ids = soc.HighSoCBatteryIds;
                    else if (key == "low") ids = soc.LowSoCBatteryIds;
                    else if (key == "chargenow") ids = soc.ChargeNowBatteryIds;

                    var idSet = new HashSet<long>(ids ?? new List<long>());
                    var (items, total) = popupRepo.GetPopupDashboardRows(idSet, search, pageNumber, pageSize);

                    return Json(new
                    {
                        Success = true,
                        Items = items,
                        Total = total,
                        PageNumber = pageNumber,
                        PageSize = pageSize,

                        // Must be the number of Battery_IDs for the clicked card (not page size)
                        PowerModulesCount = idSet.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetSocPopupData failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message,
                    Items = new List<DashboardPopupRowModel>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PowerModulesCount = 0
                });
            }
        }

        [HttpGet]
        public JsonResult GetCycleStatusPopupData(string cardKey, string search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUser = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));

                long? communityId = null;
                long? groupId = null;
                var includeAllSubGroups = SessionHelper.IncludeAllSubGroups ?? true;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    communityId = SessionHelper.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID;
                }
                else if (User.IsInRole("Community User"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID ?? SessionHelper.GroupID;
                }

                using (var repo = new DashboardDataRepository())
                using (var popupRepo = new DashboardPopupRepository())
                {
                    var cycle = repo.GetCycleStatus(communityId, groupId, includeAllSubGroups);

                    IEnumerable<long> ids = Enumerable.Empty<long>();
                    var key = (cardKey ?? string.Empty).Trim().ToLowerInvariant();

                    if (key == "low") ids = cycle.LowBatteryIds;
                    else if (key == "high") ids = cycle.HighBatteryIds;
                    else if (key == "eol") ids = cycle.EndOfLifeBatteryIds;

                    var idSet = new HashSet<long>(ids ?? new List<long>());
                    var (items, total) = popupRepo.GetPopupDashboardRows(idSet, search, pageNumber, pageSize);

                    return Json(new
                    {
                        Success = true,
                        Items = items,
                        Total = total,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        PowerModulesCount = idSet.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetCycleStatusPopupData failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message,
                    Items = new List<DashboardPopupRowModel>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PowerModulesCount = 0
                });
            }
        }

        [HttpGet]
        public JsonResult GetEffectiveRotationPopupData(string cardKey, string search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUser = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));

                long? communityId = null;
                long? groupId = null;
                var includeAllSubGroups = SessionHelper.IncludeAllSubGroups ?? true;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    communityId = SessionHelper.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID;
                }
                else if (User.IsInRole("Community User"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID ?? SessionHelper.GroupID;
                }

                using (var repo = new DashboardDataRepository())
                using (var popupRepo = new DashboardPopupRepository())
                {
                    var rotation = repo.GetEffectiveRotation(communityId, groupId, includeAllSubGroups);

                    IEnumerable<long> ids = Enumerable.Empty<long>();
                    var key = (cardKey ?? string.Empty).Trim().ToLowerInvariant();

                    if (key == "good") ids = rotation.GoodBatteryIds;
                    else if (key == "average") ids = rotation.AverageBatteryIds;
                    else if (key == "poor") ids = rotation.PoorBatteryIds;

                    var idSet = new HashSet<long>(ids ?? new List<long>());
                    var (items, total) = popupRepo.GetPopupDashboardRows(idSet, search, pageNumber, pageSize);

                    return Json(new
                    {
                        Success = true,
                        Items = items,
                        Total = total,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        PowerModulesCount = idSet.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetEffectiveRotationPopupData failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message,
                    Items = new List<DashboardPopupRowModel>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PowerModulesCount = 0
                });
            }
        }

        [HttpGet]
        public JsonResult GetTemperaturePopupData(string cardKey, string search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUser = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));

                long? communityId = null;
                long? groupId = null;
                var includeAllSubGroups = SessionHelper.IncludeAllSubGroups ?? true;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    communityId = SessionHelper.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID;
                }
                else if (User.IsInRole("Community User"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID ?? SessionHelper.GroupID;
                }

                using (var repo = new DashboardDataRepository())
                using (var popupRepo = new DashboardPopupRepository())
                {
                    var temp = repo.GetTemperature(communityId, groupId, includeAllSubGroups);

                    IEnumerable<long> ids = Enumerable.Empty<long>();

                    var key = (cardKey ?? string.Empty).Trim();
                    key = key.Replace(" ", string.Empty);
                    key = key.Replace("-", string.Empty);
                    key = key.ToLowerInvariant();

                    if (key == "chargingnormal") ids = temp.ChargingNormalBatteryIds;
                    else if (key == "chargingwarning") ids = temp.ChargingWarningBatteryIds;
                    else if (key == "dischargingnormal") ids = temp.DischargingNormalBatteryIds;
                    else if (key == "dischargingwarning") ids = temp.DischargingWarningBatteryIds;

                    var idSet = new HashSet<long>(ids ?? new List<long>());
                    var (items, total) = popupRepo.GetPopupDashboardRows(idSet, search, pageNumber, pageSize);

                    return Json(new
                    {
                        Success = true,
                        Items = items,
                        Total = total,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        PowerModulesCount = idSet.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetTemperaturePopupData failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message,
                    Items = new List<DashboardPopupRowModel>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PowerModulesCount = 0
                });
            }
        }

        [HttpGet]
        public JsonResult GetBatteryStatusPopupData(string cardKey, string search, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var currentUser = UserManager.FindById(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier));

                long? communityId = null;
                long? groupId = null;
                var includeAllSubGroups = SessionHelper.IncludeAllSubGroups ?? true;

                if (User.IsInRole("Application Admin") || User.IsInRole("Application User"))
                {
                    communityId = SessionHelper.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = SessionHelper.GroupID;
                }
                else if (User.IsInRole("Community Group Admin"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID;
                }
                else if (User.IsInRole("Community User"))
                {
                    communityId = currentUser.CommunityID;
                    groupId = currentUser.GroupID ?? SessionHelper.GroupID;
                }

                using (var repo = new DashboardDataRepository())
                using (var popupRepo = new DashboardPopupRepository())
                {
                    var status = repo.GetBatteryStatus(communityId, groupId, includeAllSubGroups);

                    IEnumerable<long> ids = Enumerable.Empty<long>();
                    var key = (cardKey ?? string.Empty).Trim().ToLowerInvariant();

                    if (key == "ondevicecharging") ids = status.OnDeviceChargingBatteryIds;
                    else if (key == "ondevicedischarging") ids = status.OnDeviceDischargingBatteryIds;
                    else if (key == "ondeviceidle") ids = status.OnDeviceIdleBatteryIds;
                    else if (key == "offdevicecharging") ids = status.OffDeviceChargingBatteryIds;
                    else if (key == "offdeviceidle") ids = status.OffDeviceIdleBatteryIds;

                    var idSet = new HashSet<long>(ids ?? new List<long>());
                    var (items, total) = popupRepo.GetPopupDashboardRows(idSet, search, pageNumber, pageSize);

                    return Json(new
                    {
                        Success = true,
                        Items = items,
                        Total = total,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        PowerModulesCount = idSet.Count
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "GetBatteryStatusPopupData failed.");

                return Json(new
                {
                    Success = false,
                    ErrorMessage = ex.GetBaseException().Message,
                    Items = new List<DashboardPopupRowModel>(),
                    Total = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    PowerModulesCount = 0
                });
            }
        }
    }
}