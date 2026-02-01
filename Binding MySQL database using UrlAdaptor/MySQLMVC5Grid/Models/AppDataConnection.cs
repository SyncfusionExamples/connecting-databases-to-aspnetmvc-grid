using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.MySql;
using LinqToDB.DataProvider.SqlServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MySQLMVC5Grid.Models
{
    public sealed class AppDataConnection : DataConnection
    {
        //[Obsolete]
        public AppDataConnection() :
            base(new DataOptions()
                .UseMySql(
                    ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString,
                    MySqlVersion.MySql80,
                    MySqlProvider.MySqlConnector))
            //base(MySqlTools.GetDataProvider(MySqlVersion.MySql80, MySqlProvider.MySqlConnector),
            //     ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString)
        {
            this.InlineParameters = true; // handy for SQL logging in debug

            // Remove or replace the following lines, as LinqToDB.Configuration.Configuration and
            // LinqToDB.Configuration.QueryTraceOptions do not exist in recent linq2db versions.
            // Tracing can be enabled via DataConnection.OnTrace event or by configuring logging
            // through other means, depending on your linq2db version.

            // Example: Enable tracing using OnTrace event (uncomment if needed)
            // this.OnTrace = info => System.Diagnostics.Debug.WriteLine(info.SqlText);

            // If you want to write trace lines, you can uncomment and implement the following:
            // LinqToDB.Configuration.Configuration.QueryTrace.WriteLine = (msg, cat, lvl) =>
            //     System.Diagnostics.Debug.WriteLine($"[linq2db] {msg}");
        }

        public ITable<Transactions> Transactions => this.GetTable<Transactions>();
    }
}