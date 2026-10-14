namespace Acme.Workforce.Compliance.Exception.Preference;

using Acme.Workforce.Compliance.Audit.Policy;

/// <summary>Functions of pending exception quota from policy.</summary>
public static class PendingExceptionQuotaFromPolicy
{
    public static Dictionary<string, double> SummarizeAuditPoliciesForQuota(List<AuditPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<PendingExceptionQuota> DerivePendingExceptionQuotasFromPolicy(List<AuditPolicy> items)
    {
        var outValue = new List<PendingExceptionQuota>();
        foreach (var item in items)
        {
            var derived = PendingExceptionQuota.NewPendingExceptionQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizePendingExceptionQuota(PendingExceptionQuota item)
    {
        return AuditPolicyPolicy.AuditPolicyWeight(1, true);
    }
}
