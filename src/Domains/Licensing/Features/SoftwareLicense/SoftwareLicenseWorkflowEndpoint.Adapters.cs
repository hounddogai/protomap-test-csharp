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
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLicensing = Acme.Licensing.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiTelematics = Acme.Telematics.V1;

namespace Acme.Workforce.Licensing;

public static class SoftwareLicenseWorkflowEndpointRequests
{
    /// <summary>
    /// Builds the NormalizeDocumentFolder request from the current workflow state.
    /// </summary>
    public static ApiDocuments.NormalizeDocumentFolderRequest StartSoftwareLicenseWorkflowToNormalizeDocumentFolder1(this ApiLicensing.StartSoftwareLicenseWorkflowRequest request, ApiEmployees.ValidateEmploymentContractResponse reply0)
    {
        return new ApiDocuments.NormalizeDocumentFolderRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            DocumentFolder = new ApiDocuments.DocumentFolder
            {
                Children = { new ApiDocuments.DocumentFolder
                {
                    Children = { new ApiDocuments.DocumentFolder
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedBy = (reply0.EmploymentContract?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Locality = (reply0.EmploymentContract?.BillingAddress?.Locality ?? ""),
                        },
                    } },
                } },
            },
        };
    }
}
