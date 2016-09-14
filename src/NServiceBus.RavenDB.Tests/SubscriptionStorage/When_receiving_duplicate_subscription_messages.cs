using System.Linq;
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
public class When_receiving_duplicate_subscription_messages : RavenDBPersistenceTestBase
{
    [Test]
    public async Task should_not_create_additional_db_rows()
    {
        var storage = new SubscriptionPersister(store);

        await storage.Subscribe(new Subscriber("testEndPoint@localhost", "testEndPoint"), new MessageType("SomeMessageType", "1.0.0.0"), new ContextBag());

        await storage.Subscribe(new Subscriber("testEndPoint@localhost", "testEndPoint"), new MessageType("SomeMessageType", "1.0.0.0"), new ContextBag());

        using (var session = store.OpenAsyncSession())
        {
            var subscriptions = await session
                .Query<Subscription>()
                .Customize(c => c.WaitForNonStaleResults())
                .CountAsync();

            Assert.AreEqual(1, subscriptions);
        }
    }

    [Test]
    public async Task should_overwrite_existing_subscription()
    {
        const string subscriberAddress = "testEndPoint@localhost";
        var messageType = new MessageType("SomeMessageType", "1.0.0.0");

        var storage = new SubscriptionPersister(store);
        await storage.Subscribe(new Subscriber(subscriberAddress, "old"), messageType, new ContextBag());
        await storage.Subscribe(new Subscriber(subscriberAddress, "new"), messageType, new ContextBag());

        var subscriber = (await storage.GetSubscriberAddressesForMessage(new[]
        {
            messageType
        }, new ContextBag())).ToArray();

        Assert.AreEqual(1, subscriber.Length);
        Assert.AreEqual(subscriberAddress, subscriber[0].TransportAddress);
        Assert.AreEqual("new", subscriber[0].Endpoint);
    }
}