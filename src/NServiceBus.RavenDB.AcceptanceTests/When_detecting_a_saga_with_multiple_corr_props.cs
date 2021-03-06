﻿namespace NServiceBus.RavenDB.AcceptanceTests
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_detecting_a_saga_with_multiple_corr_props : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_blow_up()
        {
            Exception exceptionToVerify = null;

            try
            {
                await Scenario.Define<Context>()
                    .WithEndpoint<MultiPropEndpoint>(e => e.DoNotFailOnErrorMessages())
                    .Done(c => c.EndpointsStarted)
                    .Run();
            }
            catch (Exception ex)
            {
                exceptionToVerify = ex;
            }
            Assert.IsNotNull(exceptionToVerify);

            const string expectedMessage = "Sagas can only have mappings that correlate on a single saga property. Use custom finders to correlate";

            Assert.True(exceptionToVerify.Message.Contains(expectedMessage), "Should tell user to use a single correlation property or custom finders");
        }

        public class Context : ScenarioContext
        {
        }

        public class MultiPropEndpoint : EndpointConfigurationBuilder
        {
            public MultiPropEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class MultiPropSaga : Saga<MultiPropSagaData>, IAmStartedByMessages<StartSagaMessage>
            {
                public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MultiPropSagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSagaMessage>(m => m.SomeId).ToSaga(s => s.SomeId);
                    mapper.ConfigureMapping<StartSagaMessage2>(m => m.SomeOtherId).ToSaga(s => s.SomeOtherId);
                }
            }

            public class MultiPropSagaData : IContainSagaData
            {
                public Guid SomeOtherId { get; set; }
                public Guid SomeId { get; set; }
                public Guid Id { get; set; }
                public string Originator { get; set; }
                public string OriginalMessageId { get; set; }
            }
        }

        public class StartSagaMessage : ICommand
        {
            public Guid SomeId { get; set; }
        }

        public class StartSagaMessage2 : ICommand
        {
            public Guid SomeOtherId { get; set; }
        }
    }
}