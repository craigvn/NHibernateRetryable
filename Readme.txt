
Change the NHibernate config to something like

<property name="connection.provider">YoureOnTime.Data.RetryConnectionStringProvider, YoureOnTime.Common</property>
<property name="connection.driver_class">YoureOnTime.Data.RetrySqlClientDriver, YoureOnTime.Common</property>
