using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.StorageClient;
using YoureOnTime.Common;

namespace YoureOnTime.Data
{
    [System.ComponentModel.DesignerCategory("")]
    public class RetryDbCommand : DbCommand
    {
        protected DbCommand _cmd = new SqlCommand();
        protected DbConnection _conn = null;
        protected DbTransaction _tran = null;

        public RetryDbCommand()
        {
            this.RetryPolicy = RetryPolicies.Retry(MAX_RETRIES, TimeSpan.FromMilliseconds(WAIT_BETWEEN_RETRIES));
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

        public override string CommandText
        {
            get { return _cmd.CommandText; }
            set { _cmd.CommandText = value; }
        }

        public override int CommandTimeout
        {
            get { return _cmd.CommandTimeout; }
            set { _cmd.CommandTimeout = value; }
        }

        public override CommandType CommandType
        {
            get { return _cmd.CommandType; }
            set { _cmd.CommandType = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _conn; }
            set
            {
                _conn = value;
                _cmd.Connection = _conn;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _cmd.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return _tran; }
            set
            {
                this._tran = value;
                _cmd.Transaction = _tran;
            }
        }

        public override bool DesignTimeVisible
        {
            get { return _cmd.DesignTimeVisible; }
            set { _cmd.DesignTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _cmd.UpdatedRowSource; }
            set { _cmd.UpdatedRowSource = value; }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            DbDataReader result = null;
            try
            {
                Retry.RequestWithRetry(RetryPolicy, () =>
                {
                    result = _cmd.ExecuteReader(behavior);
                });
                return result;
            }
            catch (Exception)
            {
                if (result != null)
                    result.Dispose();
                throw;
            }
        }

        public override int ExecuteNonQuery()
        {
            int result = -1;
            try
            {
                Retry.RequestWithRetry(RetryPolicy, () =>
                {
                    result = _cmd.ExecuteNonQuery();
                });
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override object ExecuteScalar()
        {
            object result = null;
            try
            {
                Retry.RequestWithRetry(RetryPolicy, () =>
                {
                    result = _cmd.ExecuteScalar();
                });
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public override void Cancel()
        {
            _cmd.Cancel();
        }

        public override void Prepare()
        {
            _cmd.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return _cmd.CreateParameter();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _cmd != null)
            {
                _cmd.Dispose();
            }
            _cmd = null;
            base.Dispose(disposing);
        }

        public DbCommand InternalCommand { get { return _cmd; } }

    }

}
