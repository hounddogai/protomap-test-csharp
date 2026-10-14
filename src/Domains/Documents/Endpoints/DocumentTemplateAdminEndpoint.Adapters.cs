using Acme.Workforce.Platform;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiAuthorization = Acme.Authorization.V1;
using ApiCommon = Acme.Common.V1;
using ApiDocuments = Acme.Documents.V1;

namespace Acme.Workforce.Documents;

public static class DocumentTemplateAdminEndpointRequests
{
    /// <summary>
    /// Builds the ResolveAccessPolicies request from the current workflow state.
    /// </summary>
    public static ApiAuthorization.ResolveAccessPoliciesRequest ReassignDocumentTemplateToResolveAccessPolicies0(this ApiDocuments.ReassignDocumentTemplateRequest request)
    {
        return new ApiAuthorization.ResolveAccessPoliciesRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
