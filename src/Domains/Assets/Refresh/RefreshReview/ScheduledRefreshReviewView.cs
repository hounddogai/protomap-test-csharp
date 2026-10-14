namespace Acme.Workforce.Assets.Refresh.RefreshReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled refresh review.</summary>
public sealed record ScheduledRefreshReviewView(string CodeText, string TitleText, string ManagerId, string Revision, string CapacityText, string Summary, string TenureMonthsText, string StatusLabel)
{
    public static ScheduledRefreshReviewView ToScheduledRefreshReviewView(ScheduledRefreshReview item)
    {
        return new ScheduledRefreshReviewView(
            item.Code,
            item.Title,
            Text.Truncate(item.ManagerId, 40),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRefreshReviewView> ToScheduledRefreshReviewViews(List<ScheduledRefreshReview> items)
    {
        var views = new List<ScheduledRefreshReviewView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRefreshReviewView(item));
        }
        return views;
    }

    public static double SumScheduledRefreshReviewView(ScheduledRefreshReviewView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedCapacity + (double)parsedTenureMonths;
    }
}
