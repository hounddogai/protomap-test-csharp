namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Compliance.PolicyAck.AckCycle;

/// <summary>Functions of certification notice from rollup.</summary>
public static class CertificationNoticeFromRollup
{
    public static Dictionary<string, double> SummarizeCappedPolicyAckRollupsForNotice(List<CappedPolicyAckRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<CertificationNotice> DeriveCertificationNoticesFromRollup(List<CappedPolicyAckRollup> items)
    {
        var outValue = new List<CertificationNotice>();
        foreach (var item in items)
        {
            var derived = CertificationNotice.NewCertificationNotice(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyCertificationNotice(CertificationNotice item)
    {
        return SeasonalPolicyAckOverridePolicy.NormalizeSeasonalPolicyAckOverrideCode(item.Code);
    }
}
