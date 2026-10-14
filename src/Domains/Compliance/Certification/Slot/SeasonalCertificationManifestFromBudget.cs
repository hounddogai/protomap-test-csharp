namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Functions of seasonal certification manifest from budget.</summary>
public static class SeasonalCertificationManifestFromBudget
{
    public static Dictionary<string, double> SummarizeStrictAttestationBudgetsForManifest(List<StrictAttestationBudget> items)
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

    public static List<SeasonalCertificationManifest> DeriveSeasonalCertificationManifestsFromBudget(List<StrictAttestationBudget> items)
    {
        var outValue = new List<SeasonalCertificationManifest>();
        foreach (var item in items)
        {
            var derived = SeasonalCertificationManifest.NewSeasonalCertificationManifest(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateSeasonalCertificationManifest(SeasonalCertificationManifest item)
    {
        return FallbackAttestationSummaryPolicy.NormalizeFallbackAttestationSummaryCode(item.Code);
    }
}
