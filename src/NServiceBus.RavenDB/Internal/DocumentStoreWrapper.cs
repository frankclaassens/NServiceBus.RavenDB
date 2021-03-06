﻿namespace NServiceBus.Persistence.RavenDB
{
    using Raven.Client;

    class DocumentStoreWrapper : IDocumentStoreWrapper
    {
        public DocumentStoreWrapper(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }

        public IDocumentStore DocumentStore { get; }
    }
}