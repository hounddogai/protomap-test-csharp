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
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiNotifications = Acme.Notifications.V1;
using ApiPersonalization = Acme.Personalization.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiSearch = Acme.Search.V1;
using ApiTimekeeping = Acme.Timekeeping.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Recruiting;

public static class JobOpeningCalculationEndpointRequests
{
    /// <summary>
    /// Builds the CompareBuildingOptions request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.CompareBuildingOptionsRequest ExplainJobOpeningCalculationToCompareBuildingOptions0(this ApiRecruiting.ExplainJobOpeningCalculationRequest request)
    {
        return new ApiWorkplaces.CompareBuildingOptionsRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            CreatedAt = (request.JobOpening?.Audit?.CreatedAt ?? new Timestamp()),
                            CreatedBy = (request.JobOpening?.Audit?.CreatedBy ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Lines = { (request.JobOpening?.BillingAddress?.Lines ?? new Google.Protobuf.Collections.RepeatedField<string>()) },
                            Recipient = (request.JobOpening?.BillingAddress?.Recipient ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
