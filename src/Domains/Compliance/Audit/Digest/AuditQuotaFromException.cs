namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>Functions of audit quota from exception.</summary>
public static class AuditQuotaFromException
{
    public static Dictionary<string, double> SummarizeDefaultPolicyAckExceptionsForQuota(List<DefaultPolicyAckException> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<AuditQuota> DeriveAuditQuotasFromException(List<DefaultPolicyAckException> items)
    {
        var outValue = new List<AuditQuota>();
        foreach (var item in items)
        {
            var derived = AuditQuota.NewAuditQuota(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreAuditQuota(AuditQuota item)
    {
        return PolicyAckMetricPolicy.NormalizePolicyAckMetricCode(item.Code);
    }
}
