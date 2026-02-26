using GeniView.Cloud.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeniView.Cloud.Common
{
    public class DBHelper : IDisposable
    {
        private GeniViewCloudDataRepository _db;

        public GeniViewCloudDataRepository DB
        {
            get { return _db; }
            set { _db = value; }
        }

        public DBHelper()
        {
            _db = new GeniViewCloudDataRepository();
        }

        public DBHelper(GeniViewCloudDataRepository db)
        {
            _db = db;
        }

        public virtual void BatchInsert<TContext, T>(TContext db, DbSet<T> dbSet, List<T> dataList)
            where TContext : DbContext
            where T : class
        {
            if (dataList.Any())
            {
                db.AddRange(dataList);
                db.SaveChanges();
            }
        }

        public virtual void UpdateAll<TContext, T>(TContext db, DbSet<T> dbSet, List<T> dataList)
            where TContext : DbContext
            where T : class
        {
            if (dataList.Any())
            {
                db.UpdateRange(dataList);
                db.SaveChanges();
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
