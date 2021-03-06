using Raven.Abstractions.Replication;
using Raven.Client.Connection.Async;
using Raven.Client.Document;
using Raven.Tests.Common;
using Raven.Tests.Document;

using Xunit;

namespace Raven.SlowTests.Replication
{
    public class ReadStriping : ReplicationBase
    {
        [Fact]
        public void When_replicating_can_do_read_striping()
        {
            var store1 = CreateStore();
            var store2 = CreateStore();
            var store3 = CreateStore();

            using (var session = store1.OpenSession())
            {
                session.Store(new Company());
                session.SaveChanges();
            }

            SetupReplication(store1.DatabaseCommands, store2, store3);

            WaitForDocument(store2.DatabaseCommands, "companies/1");
            WaitForDocument(store3.DatabaseCommands, "companies/1");

            PauseReplicationAsync(0, store1.DefaultDatabase).Wait();
            PauseReplicationAsync(1, store2.DefaultDatabase).Wait();
            PauseReplicationAsync(2, store3.DefaultDatabase).Wait();

            using(var store = new DocumentStore
            {
                Url = store1.Url,
                Conventions =
                    {
                        FailoverBehavior = FailoverBehavior.ReadFromAllServers
                    },
                    DefaultDatabase = store1.DefaultDatabase
            })
            {
                store.Initialize();
                var replicationInformerForDatabase = store.GetReplicationInformerForDatabase();
                replicationInformerForDatabase.UpdateReplicationInformationIfNeededAsync((AsyncServerClient)store.AsyncDatabaseCommands)
                    .Wait();
                Assert.Equal(2, replicationInformerForDatabase.ReplicationDestinationsUrls.Count);

                foreach (var ravenDbServer in servers)
                {
                    ravenDbServer.Server.ResetNumberOfRequests();
                }

                for (int i = 0; i < 6; i++)
                {
                    using(var session = store.OpenSession())
                    {
                        Assert.NotNull(session.Load<Company>("companies/1"));
                    }
                }
            }
            foreach (var ravenDbServer in servers)
            {
                Assert.Equal(2, ravenDbServer.Server.NumberOfRequests);
            }
        }

        [Fact]
        public void Can_avoid_read_striping()
        {
            var store1 = CreateStore();
            var store2 = CreateStore();
            var store3 = CreateStore();

            using (var session = store1.OpenSession())
            {
                session.Store(new Company());
                session.SaveChanges();
            }

            SetupReplication(store1.DatabaseCommands, store2, store3);

            WaitForDocument(store2.DatabaseCommands, "companies/1");
            WaitForDocument(store3.DatabaseCommands, "companies/1");

            PauseReplicationAsync(0, store1.DefaultDatabase).Wait();
            PauseReplicationAsync(1, store2.DefaultDatabase).Wait();
            PauseReplicationAsync(2, store3.DefaultDatabase).Wait();

            using (var store = new DocumentStore
            {
                Url = store1.Url,
                Conventions =
                {
                    FailoverBehavior = FailoverBehavior.ReadFromAllServers
                },
                DefaultDatabase = store1.DefaultDatabase
            })
            {
                store.Initialize();
                var replicationInformerForDatabase = store.GetReplicationInformerForDatabase();
                replicationInformerForDatabase.UpdateReplicationInformationIfNeededAsync((AsyncServerClient)store.AsyncDatabaseCommands)
                    .Wait();
                Assert.Equal(2, replicationInformerForDatabase.ReplicationDestinationsUrls.Count);

                foreach (var ravenDbServer in servers)
                {
                    ravenDbServer.Server.ResetNumberOfRequests();
                }

                for (int i = 0; i < 6; i++)
                {
                    using (var session = store.OpenSession(new OpenSessionOptions
                    {
                        ForceReadFromMaster = true
                    }))
                    {
                        Assert.NotNull(session.Load<Company>("companies/1"));
                    }
                }
            }
            Assert.Equal(6, servers[0].Server.NumberOfRequests);
            Assert.Equal(0, servers[1].Server.NumberOfRequests);
            Assert.Equal(0, servers[2].Server.NumberOfRequests);
        }
    }
}
