namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Functions of certification override from variance.</summary>
public static class CertificationOverrideFromVariance
{
    public static Dictionary<string, double> SummarizeTieredTrainingDueVariancesForOverride(List<TieredTrainingDueVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<CertificationOverride> DeriveCertificationOverridesFromVariance(List<TieredTrainingDueVariance> items)
    {
        var outValue = new List<CertificationOverride>();
        foreach (var item in items)
        {
            var derived = CertificationOverride.NewCertificationOverride(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateCertificationOverride(CertificationOverride item)
    {
        return TieredTrainingDueVariancePolicy.NormalizeTieredTrainingDueVarianceCode(item.Code);
    }
}
