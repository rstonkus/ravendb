﻿using System.Net.Http;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Operations;
using Raven.Client.Http;
using Raven.Client.Json.Converters;
using Sparrow.Json;

namespace Raven.Client.ServerWide.Operations.ConnectionStrings
{
    public class RemoveConnectionStringOperation<T> : IMaintenanceOperation<RemoveConnectionStringResult> where T : ConnectionString
    {
        private readonly T _connectionString;

        public RemoveConnectionStringOperation(T connectionString)
        {
            _connectionString = connectionString;
        }

        public RavenCommand<RemoveConnectionStringResult> GetCommand(DocumentConventions conventions, JsonOperationContext context)
        {
            return new RemoveConnectionStringCommand(_connectionString);
        }

        public class RemoveConnectionStringCommand : RavenCommand<RemoveConnectionStringResult>
        {
            private readonly T _connectionString;

            public RemoveConnectionStringCommand(T connectionString)
            {
                _connectionString = connectionString;
            }

            public override bool IsReadRequest => false;

            public override HttpRequestMessage CreateRequest(JsonOperationContext ctx, ServerNode node, out string url)
            {
                url = $"{node.Url}/databases/{node.Database}/admin/connection-strings?connectionString={_connectionString.Name}&type={_connectionString.Type}";

                var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Delete
                    };

                return request;
            }

            public override void SetResponse(BlittableJsonReaderObject response, bool fromCache)
            {
                if (response == null)
                    ThrowInvalidResponse();

                Result = JsonDeserializationClient.RemoveConnectionStringResult(response);
            }
        }
    }

    public class RemoveConnectionStringResult
    {
        public long? ETag { get; set; }
    }
}