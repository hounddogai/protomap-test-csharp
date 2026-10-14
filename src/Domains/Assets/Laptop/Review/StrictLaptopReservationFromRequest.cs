namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Assets.Assignment.Request;

/// <summary>Functions of strict laptop reservation from request.</summary>
public static class StrictLaptopReservationFromRequest
{
    public static Dictionary<string, double> SummarizeAssignmentRequestsForReservation(List<AssignmentRequest> items)
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

    public static List<StrictLaptopReservation> DeriveStrictLaptopReservationsFromRequest(List<AssignmentRequest> items)
    {
        var outValue = new List<StrictLaptopReservation>();
        foreach (var item in items)
        {
            var derived = StrictLaptopReservation.NewStrictLaptopReservation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveStrictLaptopReservation(StrictLaptopReservation item)
    {
        return AssignmentRequestPolicy.AssignmentRequestBucket(item.MeasuredValue());
    }
}
