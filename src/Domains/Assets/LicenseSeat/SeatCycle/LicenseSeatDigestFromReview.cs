namespace Acme.Workforce.Assets.LicenseSeat.SeatCycle;

using Acme.Workforce.Assets.Assignment.Reservation;

/// <summary>Functions of license seat digest from review.</summary>
public static class LicenseSeatDigestFromReview
{
    public static Dictionary<string, double> SummarizeAssignmentReviewsForDigest(List<AssignmentReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<LicenseSeatDigest> DeriveLicenseSeatDigestsFromReview(List<AssignmentReview> items)
    {
        var outValue = new List<LicenseSeatDigest>();
        foreach (var item in items)
        {
            var derived = LicenseSeatDigest.NewLicenseSeatDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateLicenseSeatDigest(LicenseSeatDigest item)
    {
        return AssignmentReviewPolicy.AssignmentReviewBucket(item.WeightedLoad());
    }
}
