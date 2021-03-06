﻿namespace NServiceBus.Persistence.RavenDB
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using Raven.Client;

    class OpenAsyncSessionBehavior : Behavior<IIncomingPhysicalMessageContext>
    {
        public OpenAsyncSessionBehavior(IDocumentStoreWrapper documentStoreWrapper)
        {
            this.documentStoreWrapper = documentStoreWrapper;
        }

        public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            using (var session = OpenAsyncSession(context))
            {
                context.Extensions.Set(session);
                await next().ConfigureAwait(false);
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        IAsyncDocumentSession OpenAsyncSession(IIncomingPhysicalMessageContext context)
        {
            var databaseName = GetDatabaseName(context.Message.Headers);
            var documentSession = string.IsNullOrEmpty(databaseName)
                ? documentStoreWrapper.DocumentStore.OpenAsyncSession()
                : documentStoreWrapper.DocumentStore.OpenAsyncSession(databaseName);

            documentSession.Advanced.AllowNonAuthoritativeInformation = false;
            documentSession.Advanced.UseOptimisticConcurrency = true;

            return documentSession;
        }

        readonly IDocumentStoreWrapper documentStoreWrapper;

        public static Func<IDictionary<string, string>, string> GetDatabaseName = context => string.Empty;
    }
}