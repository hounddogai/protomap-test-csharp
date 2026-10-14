namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Functions of strict scenario quota from window.</summary>
public static class StrictScenarioQuotaFromWindow
{
    public static Dictionary<string, double> SummarizeDefaultScenarioWindowsForQuota(List<DefaultScenarioWindow> items)
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

    public static List<StrictScenarioQuota> DeriveStrictScenarioQuotasFromWindow(List<DefaultScenarioWindow> items)
    {
        var outValue = new List<StrictScenarioQuota>();
        foreach (var item in items)
        {
            var derived = StrictScenarioQuota.NewStrictScenarioQuota(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignStrictScenarioQuota(StrictScenarioQuota item)
    {
        return ManualScenarioRulePolicy.NormalizeManualScenarioRuleCode(item.Code);
    }
}
