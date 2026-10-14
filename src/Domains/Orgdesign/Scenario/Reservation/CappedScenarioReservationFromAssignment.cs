namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Functions of capped scenario reservation from assignment.</summary>
public static class CappedScenarioReservationFromAssignment
{
    public static Dictionary<string, double> SummarizePrimaryHeadcountAssignmentsForReservation(List<PrimaryHeadcountAssignment> items)
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

    public static List<CappedScenarioReservation> DeriveCappedScenarioReservationsFromAssignment(List<PrimaryHeadcountAssignment> items)
    {
        var outValue = new List<CappedScenarioReservation>();
        foreach (var item in items)
        {
            var derived = CappedScenarioReservation.NewCappedScenarioReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCappedScenarioReservation(CappedScenarioReservation item)
    {
        return DefaultHeadcountProfilePolicy.DefaultHeadcountProfileBucket(item.NetExposure());
    }
}
