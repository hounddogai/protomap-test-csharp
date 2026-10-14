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
using ApiCorporateevents = Acme.Corporateevents.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiFeatureflags = Acme.Featureflags.V1;
using ApiHelpdesk = Acme.Helpdesk.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Workplaces;

public static class DeskCalculationEndpointRequests
{
    /// <summary>
    /// Builds the ScheduleDocumentExport request from the current workflow state.
    /// </summary>
    public static ApiDocuments.ScheduleDocumentExportRequest CalculateDeskToScheduleDocumentExport1(this ApiWorkplaces.CalculateDeskRequest request)
    {
        return new ApiDocuments.ScheduleDocumentExportRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
    }
    /// <summary>
    /// Builds the CompareBuildingOptions request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.CompareBuildingOptionsRequest SimulateDeskToCompareBuildingOptions0(this ApiWorkplaces.SimulateDeskRequest request)
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
                            UpdatedAt = (request.Desk?.Audit?.UpdatedAt ?? new Timestamp()),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Region = (request.Details?.Destination?.Region ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
