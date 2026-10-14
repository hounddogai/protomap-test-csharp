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
using ApiCommon = Acme.Common.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiRecruiting = Acme.Recruiting.V1;

namespace Acme.Workforce.Helpdesk;

public static class EscalationValidationEndpointRequests
{
    /// <summary>
    /// Builds the CheckKnowledgeArticleEligibility request from the current workflow state.
    /// </summary>
    public static ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest ValidateEscalationToCheckKnowledgeArticleEligibility1(this ApiHelpdesk.ValidateEscalationRequest request, ApiDocuments.UnsubscribeDocumentFolderUpdatesResponse reply0)
    {
        return new ApiHelpdesk.CheckKnowledgeArticleEligibilityRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            KnowledgeArticle = new ApiHelpdesk.KnowledgeArticle
            {
                BillingAddress = new ApiCommon.PostalAddress
                {
                    Locality = (reply0.DocumentFolder?.Location?.Locality ?? ""),
                },
            },
        };
    }
}
