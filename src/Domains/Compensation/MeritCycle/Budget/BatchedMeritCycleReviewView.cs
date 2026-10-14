namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched merit cycle review.</summary>
public sealed record BatchedMeritCycleReviewView(string Code, string Title, string ManagerId, string Region, string TenureMonthsText, string Revision, string Rate, string Fte, string StartsAtText, string StatusLabel)
{
    public static BatchedMeritCycleReviewView ToBatchedMeritCycleReviewView(BatchedMeritCycleReview item)
    {
        return new BatchedMeritCycleReviewView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.ManagerId, 40),
            item.Region,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedMeritCycleReviewView> ToBatchedMeritCycleReviewViews(List<BatchedMeritCycleReview> items)
    {
        var views = new List<BatchedMeritCycleReviewView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedMeritCycleReviewView(item));
        }
        return views;
    }

    public static double SumBatchedMeritCycleReviewView(BatchedMeritCycleReviewView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedRevision + parsedRate;
    }
}
