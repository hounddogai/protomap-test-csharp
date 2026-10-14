namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Contractors.Rate.Mapping;

/// <summary>Functions of offboarding agreement from mapping.</summary>
public static class OffboardingAgreementFromMapping
{
    public static Dictionary<string, double> SummarizeStrictRateMappingsForAgreement(List<StrictRateMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<OffboardingAgreement> DeriveOffboardingAgreementsFromMapping(List<StrictRateMapping> items)
    {
        var outValue = new List<OffboardingAgreement>();
        foreach (var item in items)
        {
            var derived = OffboardingAgreement.NewOffboardingAgreement(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreOffboardingAgreement(OffboardingAgreement item)
    {
        return StrictRateMappingPolicy.ClassifyStrictRateMapping(item.MeasuredValue());
    }
}
