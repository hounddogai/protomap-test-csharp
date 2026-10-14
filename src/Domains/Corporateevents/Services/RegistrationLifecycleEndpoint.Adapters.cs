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
using ApiIdentity = Acme.Identity.V1;
using ApiItsm = Acme.Itsm.V1;
using ApiPatients = Acme.Patients.V1;

namespace Acme.Workforce.Corporateevents;

public static class RegistrationLifecycleEndpointRequests
{
    /// <summary>
    /// Builds the SummarizeCorporateEvents request from the current workflow state.
    /// </summary>
    public static ApiCorporateevents.SummarizeCorporateEventsRequest ExpireRegistrationToSummarizeCorporateEvents0(this ApiCorporateevents.ExpireRegistrationRequest request)
    {
        return new ApiCorporateevents.SummarizeCorporateEventsRequest
        {
            Context = (request.Context ?? new ApiCommon.RequestContext()),
            Reason = (request.Reason ?? ""),
        };
    }
}
