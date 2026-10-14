namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Functions of scenario audit from summary.</summary>
public static class ScenarioAuditFromSummary
{
    public static Dictionary<string, double> SummarizeRollingReorgSummariesForAudit(List<RollingReorgSummary> items)
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

    public static List<ScenarioAudit> DeriveScenarioAuditsFromSummary(List<RollingReorgSummary> items)
    {
        var outValue = new List<ScenarioAudit>();
        foreach (var item in items)
        {
            var derived = ScenarioAudit.NewScenarioAudit(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchScenarioAudit(ScenarioAudit item)
    {
        return PrimaryReorgClaimPolicy.ClassifyPrimaryReorgClaim(item.EffectiveLoad());
    }
}
