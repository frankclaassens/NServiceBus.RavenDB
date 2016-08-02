using System.Linq;
using System.Threading.Tasks;
using NServiceBus.Extensibility;
using NServiceBus.Persistence.RavenDB;
using NServiceBus.RavenDB.Tests;
using NUnit.Framework;

[TestFixture]
public class When_unsubscribing : RavenDBPersistenceTestBase
{
    [Test]
    public async Task subscription_entries_for_specified_message_types_should_be_removed()
    {
        var storage = new SubscriptionPersister(store);
        var context = new ContextBag();

        await storage.Subscribe(TestClients.ClientA, MessageTypes.MessageA, context);
        await storage.Subscribe(TestClients.ClientA, MessageTypes.MessageB, context);

        var clients = await storage.GetSubscriberAddressesForMessage(new[] { MessageTypes.MessageA, MessageTypes.MessageB }, context);
        Assert.That(clients.Count(), Is.EqualTo(1));

        await storage.Unsubscribe(TestClients.ClientA, MessageTypes.MessageA, context);

        clients = await storage.GetSubscriberAddressesForMessage(new []{ MessageTypes.MessageA, MessageTypes.MessageB }, context);
        Assert.That(clients.Count(), Is.EqualTo(1));

        await storage.Unsubscribe(TestClients.ClientA, MessageTypes.MessageB, context);

        clients = await storage.GetSubscriberAddressesForMessage(new[] { MessageTypes.MessageA, MessageTypes.MessageB }, context);
        Assert.That(clients, Is.Empty);
    }
}