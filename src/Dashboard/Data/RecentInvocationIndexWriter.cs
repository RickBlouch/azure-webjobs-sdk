﻿using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Dashboard.Data
{
    public class RecentInvocationIndexWriter : IRecentInvocationIndexWriter
    {
        private readonly IVersionedTextStore _store;

        [CLSCompliant(false)]
        public RecentInvocationIndexWriter(CloudBlobClient client)
            : this(VersionedTextStore.CreateBlobStore(
                client, DashboardContainerNames.Dashboard, DashboardDirectoryNames.RecentFunctionsFlat))
        {
        }

        private RecentInvocationIndexWriter(IVersionedTextStore store)
        {
            _store = store;
        }

        public void CreateOrUpdate(DateTimeOffset timestamp, Guid id)
        {
            string innerId = CreateInnerId(timestamp, id);
            _store.CreateOrUpdate(innerId, String.Empty);
        }

        public void DeleteIfExists(DateTimeOffset timestamp, Guid id)
        {
            string innerId = CreateInnerId(timestamp, id);
            _store.DeleteIfExists(innerId);
        }

        private static string CreateInnerId(DateTimeOffset timestamp, Guid id)
        {
            return RecentInvocationEntry.Format(timestamp, id);
        }
    }
}