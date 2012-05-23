using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.WindowsAzure.StorageClient;
using NHibernate;
using NHibernate.Connection;
using YoureOnTime.Common;
using YoureOnTime.Encryption;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace YoureOnTime.Data
{
    public class RetryConnectionStringProvider : DriverConnectionProvider
    {
        public RetryConnectionStringProvider()
        {
            this.RetryPolicy = RetryPolicies.Retry(MAX_RETRIES, TimeSpan.FromMilliseconds(WAIT_BETWEEN_RETRIES));
        }

        private string m_ConnectionString = "";

        public override void Configure(IDictionary<string, string> settings)
        {
            if (RoleEnvironment.IsAvailable)
            {
                m_ConnectionString = RoleEnvironment.GetConfigurationSettingValue("SqlConnectionString");
            }
            else
            {
                if (!settings.TryGetValue(NHibernate.Cfg.Environment.ConnectionString, out m_ConnectionString))
                    m_ConnectionString = GetNamedConnectionString(settings);
            }

            if (m_ConnectionString == null)
            {
                throw new HibernateException("Could not find connection string setting (set "
                    + NHibernate.Cfg.Environment.ConnectionString + " or "
                    + NHibernate.Cfg.Environment.ConnectionStringName + " property)");
            }
            ConfigureDriver(settings);
        }

        /// <summary>
        /// Maximum number of retries
        /// </summary>
        private const int MAX_RETRIES = 10;

        /// <summary>
        /// The retry policy for operations that can be retried (sql etc)
        /// </summary>
        private RetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Wait 3 seconds between retries. This is required for SQL Azure which can arbitrarily drop connections
        /// based upon resource patterns
        /// </summary>
        private const int WAIT_BETWEEN_RETRIES = 3000;
 
        public override IDbConnection GetConnection()
        {
            IDbConnection conn = null;
            try
            {
                Retry.RequestWithRetry(RetryPolicy, () =>
                {
                    IDbConnection connection = Driver.CreateConnection();
                    connection.ConnectionString = ConnectionString;
                    connection.Open();

                    conn = connection;
                });
                return conn;
            }
            catch (Exception)
            {
                if (conn != null)
                    conn.Dispose();
                throw;
            }
        }

        protected override string ConnectionString
        {
            //get { return Encrypter.Decrypt(m_ConnectionString); }
            get { return m_ConnectionString; }
        }
    }
}
