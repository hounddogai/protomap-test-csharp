namespace Acme.Workforce.Contractors.Timesheet.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy timesheet quota.</summary>
public sealed record LegacyTimesheetQuotaView(string Code, string Title, string Label, string Attempts, string Capacity, string RatingText, string SourceText, string Cost, string UpdatedAt, string StatusLabel)
{
    public static LegacyTimesheetQuotaView ToLegacyTimesheetQuotaView(LegacyTimesheetQuota item)
    {
        return new LegacyTimesheetQuotaView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            Text.Truncate(item.Label, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTimesheetQuotaView> ToLegacyTimesheetQuotaViews(List<LegacyTimesheetQuota> items)
    {
        var views = new List<LegacyTimesheetQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTimesheetQuotaView(item));
        }
        return views;
    }

    public static double SumLegacyTimesheetQuotaView(LegacyTimesheetQuotaView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedCapacity + parsedRating;
    }
}
