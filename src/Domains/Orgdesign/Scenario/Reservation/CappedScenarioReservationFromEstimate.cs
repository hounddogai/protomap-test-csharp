namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Functions of capped scenario reservation from estimate.</summary>
public static class CappedScenarioReservationFromEstimate
{
    public static Dictionary<string, double> SummarizeLegacyPositionEstimatesForReservation(List<LegacyPositionEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<CappedScenarioReservation> DeriveCappedScenarioReservationsFromEstimate(List<LegacyPositionEstimate> items)
    {
        var outValue = new List<CappedScenarioReservation>();
        foreach (var item in items)
        {
            var derived = CappedScenarioReservation.NewCappedScenarioReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveCappedScenarioReservation(CappedScenarioReservation item)
    {
        return LegacyPositionEstimatePolicy.LegacyPositionEstimateBucket(item.NetExposure());
    }
}
