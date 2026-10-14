namespace Acme.Workforce.Assets.Assignment.Summary;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Functions of assignment summary from reservation.</summary>
public static class AssignmentSummaryFromReservation
{
    public static Dictionary<string, double> SummarizeStrictLaptopReservationsForSummary(List<StrictLaptopReservation> items)
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

    public static List<AssignmentSummary> DeriveAssignmentSummariesFromReservation(List<StrictLaptopReservation> items)
    {
        var outValue = new List<AssignmentSummary>();
        foreach (var item in items)
        {
            var derived = AssignmentSummary.NewAssignmentSummary(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAssignmentSummary(AssignmentSummary item)
    {
        return WeightedLaptopThresholdPolicy.ClassifyWeightedLaptopThreshold(item.AdjustedTotal());
    }
}
