namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Functions of primary scenario agreement from threshold.</summary>
public static class PrimaryScenarioAgreementFromThreshold
{
    public static Dictionary<string, double> SummarizeScheduledReorgThresholdsForAgreement(List<ScheduledReorgThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<PrimaryScenarioAgreement> DerivePrimaryScenarioAgreementsFromThreshold(List<ScheduledReorgThreshold> items)
    {
        var outValue = new List<PrimaryScenarioAgreement>();
        foreach (var item in items)
        {
            var derived = PrimaryScenarioAgreement.NewPrimaryScenarioAgreement(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScorePrimaryScenarioAgreement(PrimaryScenarioAgreement item)
    {
        return ScheduledReorgThresholdPolicy.ScheduledReorgThresholdBucket(item.EffectiveVolume());
    }
}
