using GeniView.Cloud.Models;
using GeniView.Cloud.Repository;
using GeniView.Cloud.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GeniView.Cloud.Controllers.API
{
    [ApiController]
    [Route("api/appupdate")]
    public class ApplicationUpdateController : ControllerBase
    {
        /// <summary>
        /// GET /api/appupdate/checkforupdate?appId=...&amp;currentVersion=1.0.0
        /// Returns the <see cref="ApplicationUpdate"/> if a newer version is available, or 204 No Content.
        /// </summary>
        [HttpGet("checkforupdate")]
        public IActionResult CheckForUpdate(Guid appId, string currentVersion)
        {
            if (appId == Guid.Empty)
                return BadRequest("appId must be provided.");
            if (string.IsNullOrWhiteSpace(currentVersion) || !Version.TryParse(currentVersion, out var parsedVersion))
                return BadRequest("A valid currentVersion string (e.g. \"1.0.0\") must be provided.");

            var svc = new GeniViewApplicationUpdateService();
            var update = svc.CheckForUpdate(appId, parsedVersion);

            if (update == null)
                return NoContent();

            return Ok(update);
        }
    }
}
