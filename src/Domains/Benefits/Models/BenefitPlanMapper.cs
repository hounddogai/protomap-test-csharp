using Acme.Workforce.Platform;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiBenefits = Acme.Benefits.V1;

namespace Acme.Workforce.Benefits;

/// <summary>
/// The benefit plan snapshot persisted by the domain repository.
/// </summary>
public sealed record BenefitPlanSnapshot(string TenantId, string Id, byte[] Payload);

public static class BenefitPlanMapper
{
    public static StoredRecord ToRecord(string tenant, ApiBenefits.BenefitPlan value)
    {
        var snapshot = new BenefitPlanSnapshot(tenant, value.BenefitPlanId, value.ToByteArray());
        return new StoredRecord(snapshot.TenantId, "benefit_plan", snapshot.Id, snapshot.Payload);
    }
    public static ApiBenefits.BenefitPlan FromRecord(StoredRecord record) => ApiBenefits.BenefitPlan.Parser.ParseFrom(record.Payload);
}
