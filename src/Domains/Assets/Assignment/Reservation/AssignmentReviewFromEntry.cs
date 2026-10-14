namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Assets.Assignment.AssignmentEntry;

/// <summary>Functions of assignment review from entry.</summary>
public static class AssignmentReviewFromEntry
{
    public static Dictionary<string, double> SummarizePendingAssignmentEntriesForReview(List<PendingAssignmentEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AssignmentReview> DeriveAssignmentReviewsFromEntry(List<PendingAssignmentEntry> items)
    {
        var outValue = new List<AssignmentReview>();
        foreach (var item in items)
        {
            var derived = AssignmentReview.NewAssignmentReview(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAssignmentReview(AssignmentReview item)
    {
        return PendingAssignmentEntryPolicy.PendingAssignmentEntryBucket(item.AdjustedValue());
    }
}
