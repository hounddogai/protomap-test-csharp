namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Functions of default audit tier from tier.</summary>
public static class DefaultAuditTierFromTier
{
    public static Dictionary<string, double> SummarizeAuditTiersForTier(List<AuditTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<DefaultAuditTier> DeriveDefaultAuditTiersFromTier(List<AuditTier> items)
    {
        var outValue = new List<DefaultAuditTier>();
        foreach (var item in items)
        {
            var derived = DefaultAuditTier.NewDefaultAuditTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ScoreDefaultAuditTier(DefaultAuditTier item)
    {
        return AuditTierPolicy.AuditTierWeight(7, true);
    }
}
