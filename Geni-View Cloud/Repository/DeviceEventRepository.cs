using GeniView.Cloud.Common;
using Microsoft.EntityFrameworkCore;
using GeniView.Data.Hardware.Event;
using Microsoft.EntityFrameworkCore;
using NLog;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GeniView.Cloud.Repository
{
    public class DeviceEventRepository
    {
        private DBHelper _dbHelper = new DBHelper();
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public bool CreateBatch (List<DeviceEvent>events, GeniViewCloudDataRepository db)
        {
            var ret = false;
            _dbHelper.BatchInsert(db, db.DeviceEvents, events);
            ret = true;
            return ret;
        }


    }
}