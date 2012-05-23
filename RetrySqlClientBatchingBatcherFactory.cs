using NHibernate;
using NHibernate.AdoNet;
using NHibernate.Engine;

namespace YoureOnTime.Data
{
    public class RetrySqlClientBatchingBatcherFactory : IBatcherFactory
    {
        public virtual IBatcher CreateBatcher(ConnectionManager connectionManager, IInterceptor interceptor)
        {
            return new RetrySqlClientBatchingBatcher(connectionManager, interceptor);
        }
    }
}
