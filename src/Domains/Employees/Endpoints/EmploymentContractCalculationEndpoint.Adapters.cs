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
using ApiAudit = Acme.Audit.V1;
using ApiAuthorization = Acme.Authorization.V1;
using ApiBenefits = Acme.Benefits.V1;
using ApiCommon = Acme.Common.V1;
using ApiConcessions = Acme.Concessions.V1;
using ApiDevices = Acme.Devices.V1;
using ApiDispatch = Acme.Dispatch.V1;
using ApiDocuments = Acme.Documents.V1;
using ApiEmployees = Acme.Employees.V1;
using ApiIdentity = Acme.Identity.V1;
using ApiLedger = Acme.Ledger.V1;
using ApiOnboarding = Acme.Onboarding.V1;
using ApiPerformance = Acme.Performance.V1;
using ApiRecruiting = Acme.Recruiting.V1;
using ApiScheduling = Acme.Scheduling.V1;
using ApiShows = Acme.Shows.V1;
using ApiWorkplaces = Acme.Workplaces.V1;

namespace Acme.Workforce.Employees;

public static class EmploymentContractCalculationEndpointRequests
{
    /// <summary>
    /// Builds the SimulateBuilding request from the current workflow state.
    /// </summary>
    public static ApiWorkplaces.SimulateBuildingRequest RecalculateEmploymentContractsToSimulateBuilding2(this ApiEmployees.RecalculateEmploymentContractsRequest request)
    {
        return new ApiWorkplaces.SimulateBuildingRequest
        {
            Building = new ApiWorkplaces.Building
            {
                Children = { new ApiWorkplaces.Building
                {
                    Children = { new ApiWorkplaces.Building
                    {
                        Audit = new ApiCommon.AuditInfo
                        {
                            Etag = (request.EmploymentContract?.Audit?.Etag ?? ""),
                        },
                        Location = new ApiCommon.PostalAddress
                        {
                            Recipient = (request.Details?.Destination?.Recipient ?? ""),
                            Region = (request.Details?.Destination?.Region ?? ""),
                        },
                        Manager = new ApiIdentity.UserRef
                        {
                            DisplayName = (request.EmploymentContract?.Counterparty?.DisplayName ?? ""),
                        },
                    } },
                } },
            },
            Context = (request.Context ?? new ApiCommon.RequestContext()),
        };
    }
}
