namespace Acme.Workforce.Contractors.Rate.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a capped rate milestone.</summary>
public sealed record CappedRateMilestoneView(string Code, string Label, string ApprovedText, string TenureMonths, string Location, string Limit, string DurationMinutes, string Category, string GraceDays, string StatusLabel)
{
    public static CappedRateMilestoneView ToCappedRateMilestoneView(CappedRateMilestone item)
    {
        return new CappedRateMilestoneView(
            item.Code,
            item.Label,
            $"{item.Approved}",
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedRateMilestoneView> ToCappedRateMilestoneViews(List<CappedRateMilestone> items)
    {
        var views = new List<CappedRateMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToCappedRateMilestoneView(item));
        }
        return views;
    }

    public static double SumCappedRateMilestoneView(CappedRateMilestoneView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedLimit + (double)parsedDurationMinutes;
    }
}
