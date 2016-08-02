using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Persistence.RavenDB;
using NServiceBus.RavenDB.Persistence.SubscriptionStorage;
using NServiceBus.RavenDB.Tests;
using NServiceBus.Unicast.Subscriptions;
using NServiceBus.Unicast.Subscriptions.MessageDrivenSubscriptions;
using NUnit.Framework;
using Raven.Client;

[TestFixture]
public class When_subscribing : RavenDBPersistenceTestBase
{
    [Test]
    public async Task subscription_is_persisted()
    {
        var clientEndpoint = new Subscriber("TestEndpoint", "TestEndpoint");

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