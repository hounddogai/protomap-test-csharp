namespace Acme.Workforce.Compliance.Certification.Summary;

using Acme.Workforce.Compliance.PolicyAck.Quota;

/// <summary>Functions of certification summary from override.</summary>
public static class CertificationSummaryFromOverride
{
    public static Dictionary<string, double> SummarizeDefaultPolicyAckOverridesForSummary(List<DefaultPolicyAckOverride> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<CertificationSummary> DeriveCertificationSummariesFromOverride(List<DefaultPolicyAckOverride> items)
    {
        var outValue = new List<CertificationSummary>();
        foreach (var item in items)
        {
            var derived = CertificationSummary.NewCertificationSummary(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchCertificationSummary(CertificationSummary item)
    {
        return PrimaryPolicyAckExceptionPolicy.PrimaryPolicyAckExceptionWeight(4, true);
    }
}
