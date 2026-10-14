namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Contractors.Offboarding.Agreement;

/// <summary>Functions of offboarding baseline from agreement.</summary>
public static class OffboardingBaselineFromAgreement
{
    public static Dictionary<string, double> SummarizeOffboardingAgreementsForBaseline(List<OffboardingAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<OffboardingBaseline> DeriveOffboardingBaselinesFromAgreement(List<OffboardingAgreement> items)
    {
        var outValue = new List<OffboardingBaseline>();
        foreach (var item in items)
        {
            var derived = OffboardingBaseline.NewOffboardingBaseline(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyOffboardingBaselineExtra(OffboardingBaseline item)
    {
        return OffboardingAgreementPolicy.ClassifyOffboardingAgreement(item.NetExposure());
    }
}
