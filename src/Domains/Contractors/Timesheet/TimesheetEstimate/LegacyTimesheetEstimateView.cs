namespace Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy timesheet estimate.</summary>
public sealed record LegacyTimesheetEstimateView(string Code, string Title, string Hours, string NotesText, string Country, string Cost, string Sequence, string Discount, string StartsAt, string Archived, string StatusLabel)
{
    public static LegacyTimesheetEstimateView ToLegacyTimesheetEstimateView(LegacyTimesheetEstimate item)
    {
        return new LegacyTimesheetEstimateView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.Country,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTimesheetEstimateView> ToLegacyTimesheetEstimateViews(List<LegacyTimesheetEstimate> items)
    {
        var views = new List<LegacyTimesheetEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTimesheetEstimateView(item));
        }
        return views;
    }

    public static double SumLegacyTimesheetEstimateView(LegacyTimesheetEstimateView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return parsedHours + parsedCost + (double)parsedSequence;
    }
}
