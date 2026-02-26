using GeniView.Cloud.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace GeniView.Cloud.Controllers.API
{
    public class BaseApiController : ControllerBase
    {
        public GeniViewCloudDataRepository _db = new GeniViewCloudDataRepository();

        protected long GetDefaultAgentID()
        {
            var findAgent = _db.Agents.Where(a => a.Name.ToLower() == "default").FirstOrDefault();
            if (findAgent != null)
            {
                return findAgent.ID;
            }
            else
            {
                return -1;
            }
        }

        protected IActionResult ResponseErrorMessage(HttpStatusCode httpStatusCode, string errorMessage)
        {
            var jObject = new JObject
            {
                { "Message", errorMessage }
            };

            return StatusCode((int)httpStatusCode, JsonConvert.DeserializeObject(JsonConvert.SerializeObject(jObject)));
        }
    }
}
