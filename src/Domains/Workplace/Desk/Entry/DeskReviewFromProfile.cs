namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Workplace.Visitor.Cycle;

/// <summary>Functions of desk review from profile.</summary>
public static class DeskReviewFromProfile
{
    public static Dictionary<string, double> SummarizePendingVisitorProfilesForReview(List<PendingVisitorProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<DeskReview> DeriveDeskReviewsFromProfile(List<PendingVisitorProfile> items)
    {
        var outValue = new List<DeskReview>();
        foreach (var item in items)
        {
            var derived = DeskReview.NewDeskReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeDeskReview(DeskReview item)
    {
        return VisitorQuotaPolicy.NormalizeVisitorQuotaCode(item.Code);
    }
}
