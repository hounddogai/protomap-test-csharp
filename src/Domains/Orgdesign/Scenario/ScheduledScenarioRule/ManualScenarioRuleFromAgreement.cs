namespace Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.Metric;

/// <summary>Functions of manual scenario rule from agreement.</summary>
public static class ManualScenarioRuleFromAgreement
{
    public static Dictionary<string, double> SummarizePendingScenarioAgreementsForRule(List<PendingScenarioAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetValue();
        }
        return totals;
    }

    public static List<ManualScenarioRule> DeriveManualScenarioRulesFromAgreement(List<PendingScenarioAgreement> items)
    {
        var outValue = new List<ManualScenarioRule>();
        foreach (var item in items)
        {
            var derived = ManualScenarioRule.NewManualScenarioRule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateManualScenarioRule(ManualScenarioRule item)
    {
        return PendingScenarioAgreementPolicy.ClassifyPendingScenarioAgreement(item.NetVolume());
    }
}
