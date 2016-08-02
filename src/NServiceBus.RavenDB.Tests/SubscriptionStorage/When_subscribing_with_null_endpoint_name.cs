
namespace NServiceBus.RavenDB.Tests.SubscriptionStorage
{
    using System.Threading.Tasks;
    using Extensibility;
    using NServiceBus.Persistence.RavenDB;
    using RavenDB.Persistence.SubscriptionStorage;
    using Unicast.Subscriptions;
    using Unicast.Subscriptions.MessageDrivenSubscriptions;
    using NUnit.Framework;
    using Raven.Client;

    [TestFixture]
    public class When_subscribing_with_null_endpoint_name : RavenDBPersistenceTestBase
    {
        [Test]
        public async Task ensure_that_the_subscription_is_persisted()
        {
            var clientEndpoint = new Subscriber("TestEndpoint", null);

            var storage = new SubscriptionPersister(store);

            await storage.Subscribe(clientEndpoint, new MessageType("MessageType1", "1.0.0.0"), new ContextBag());

            using (var session = store.OpenAsyncSession())
            {
                var subscriptions = await session
                    .Query<Subscription>()
                    .Customize(c => c.WaitForNonStaleResults())
                    .CountAsync();

                Assert.AreEqual(1, subscriptions);
            }
        }
    }
}
