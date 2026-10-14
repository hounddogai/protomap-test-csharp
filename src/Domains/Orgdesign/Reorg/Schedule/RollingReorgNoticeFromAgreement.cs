namespace Acme.Workforce.Orgdesign.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Functions of rolling reorg notice from agreement.</summary>
public static class RollingReorgNoticeFromAgreement
{
    public static Dictionary<string, double> SummarizeScenarioAgreementsForNotice(List<ScenarioAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<RollingReorgNotice> DeriveRollingReorgNoticesFromAgreement(List<ScenarioAgreement> items)
    {
        var outValue = new List<RollingReorgNotice>();
        foreach (var item in items)
        {
            var derived = RollingReorgNotice.NewRollingReorgNotice(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergeRollingReorgNotice(RollingReorgNotice item)
    {
        return ScenarioScorePolicy.ScenarioScoreWeight(9, true);
    }
}
