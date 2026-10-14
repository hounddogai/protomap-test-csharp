namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Compliance.Attestation.RecordItem;

/// <summary>Functions of seasonal certification digest from rollup.</summary>
public static class SeasonalCertificationDigestFromRollup
{
    public static Dictionary<string, double> SummarizeDefaultAttestationRollupsForDigest(List<DefaultAttestationRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveTotal();
        }
        return totals;
    }

    public static List<SeasonalCertificationDigest> DeriveSeasonalCertificationDigestsFromRollup(List<DefaultAttestationRollup> items)
    {
        var outValue = new List<SeasonalCertificationDigest>();
        foreach (var item in items)
        {
            var derived = SeasonalCertificationDigest.NewSeasonalCertificationDigest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeSeasonalCertificationDigest(SeasonalCertificationDigest item)
    {
        return DefaultAttestationRollupPolicy.ClassifyDefaultAttestationRollup(item.NetVolume());
    }
}
