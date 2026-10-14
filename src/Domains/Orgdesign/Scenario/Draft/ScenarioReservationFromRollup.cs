namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>Functions of scenario reservation from rollup.</summary>
public static class ScenarioReservationFromRollup
{
    public static Dictionary<string, double> SummarizePositionRollupsForReservation(List<PositionRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ScenarioReservation> DeriveScenarioReservationsFromRollup(List<PositionRollup> items)
    {
        var outValue = new List<ScenarioReservation>();
        foreach (var item in items)
        {
            var derived = ScenarioReservation.NewScenarioReservation(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeScenarioReservation(ScenarioReservation item)
    {
        return PositionPolicyPolicy.PositionPolicyBucket(item.EffectiveTotal());
    }
}
