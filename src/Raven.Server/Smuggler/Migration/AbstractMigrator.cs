﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Smuggler;
using Raven.Server.Documents;
using Raven.Server.Documents.Handlers;
using Raven.Server.ServerWide;
using Sparrow.Json;

namespace Raven.Server.Smuggler.Migration
{
    public abstract class AbstractMigrator : IDisposable
    {
        protected readonly string MigrationStateKey;
        protected readonly string ServerUrl;
        protected readonly string DatabaseName;
        protected readonly SmugglerResult Result;
        protected readonly Action<IOperationProgress> OnProgress;
        protected readonly DocumentDatabase Database;
        protected readonly OperationCancelToken CancelToken;

        protected HttpClient HttpClient { get; set; }

        protected AbstractMigrator(
            string migrationStateKey,
            string serverUrl,
            string databaseName,
            SmugglerResult result,
            Action<IOperationProgress> onProgress,
            DocumentDatabase database,
            OperationCancelToken cancelToken)
        {
            MigrationStateKey = migrationStateKey;
            ServerUrl = serverUrl;
            DatabaseName = databaseName;
            Result = result;
            OnProgress = onProgress;
            Database = database;
            CancelToken = cancelToken;
        }

        public abstract Task Execute();

        public abstract void Dispose();

        protected async Task SaveLastOperationState(BlittableJsonReaderObject blittable)
        {
            var cmd = new MergedPutCommand(blittable, MigrationStateKey, null, Database);
            await Database.TxMerger.Enqueue(cmd);
        }
    }
}