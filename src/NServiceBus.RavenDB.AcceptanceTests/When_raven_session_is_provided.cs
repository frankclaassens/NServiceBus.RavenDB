﻿namespace NServiceBus.AcceptanceTests.ApiExtension
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Moq;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using Raven.Client;

    public class When_raven_session_is_provided : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task It_should_return_configured_session()
        {
            var session = Mock.Of<IAsyncDocumentSession>();

            var context =
                await Scenario.Define<RavenSessionTestContext>(testContext =>
                {
                    testContext.RavenSessionFromTest = session;
                })
                    .WithEndpoint<SharedRavenSessionExtensions>(b => b.When((bus, c) =>
                    {
                        var options = new SendOptions();

                        options.RouteToLocalEndpointInstance();

                        return bus.Send(new SharedRavenSessionExtensions.GenericMessage(), options);
                    }))
                    .Done(c => c.HandlerWasHit)
                    .Run();

            Assert.AreSame(session, context.RavenSessionFromHandler);
        }

        public class RavenSessionTestContext : ScenarioContext
        {
            public IAsyncDocumentSession RavenSessionFromTest { get; set; }
            public IAsyncDocumentSession RavenSessionFromHandler { get; set; }
            public bool HandlerWasHit { get; set; }
        }

        public class SharedRavenSessionExtensions : EndpointConfigurationBuilder
        {
            public SharedRavenSessionExtensions()
            {
                EndpointSetup<DefaultServer>((config, context) =>
                {
                    var scenarioContext = context.ScenarioContext as RavenSessionTestContext;
                    config.UsePersistence<RavenDBPersistence>().UseSharedAsyncSession(() => scenarioContext.RavenSessionFromTest);
                });
            }

            public class SharedSessionSagaData : IContainSagaData
            {
                public Guid Id { get; set; }
                public string Originator { get; set; }
                public string OriginalMessageId { get; set; }
            }

            public class SharedSessionGenericSaga : Saga<SharedSessionSagaData>, IAmStartedByMessages<GenericMessage>
            {
                public RavenSessionTestContext TestContext { get; set; }

                public Task Handle(GenericMessage message, IMessageHandlerContext context)
                {
                    TestContext.RavenSessionFromHandler = context.GetRavenSession();
                    TestContext.HandlerWasHit = true;
                    return Task.FromResult(0);
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SharedSessionSagaData> mapper)
                {
                    mapper.ConfigureMapping<GenericMessage>(m => m.Id).ToSaga(s => s.Id);
                }
            }

            [Serializable]
            public class GenericMessage : IMessage
            {
                public Guid Id { get; set; }
            }
        }
    }
}
