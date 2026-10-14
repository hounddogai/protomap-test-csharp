namespace Acme.Workforce.Contractors.Timesheet.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched timesheet tier.</summary>
public sealed record BatchedTimesheetTierView(string Code, string Label, string Minimum, string Approved, string MaximumText, string Score, string Currency, string Visible, string Reviewer, string Locale, string Quantity, string StatusLabel)
{
    public static BatchedTimesheetTierView ToBatchedTimesheetTierView(BatchedTimesheetTier item)
    {
        return new BatchedTimesheetTierView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Visible}",
            item.Reviewer,
            item.Locale,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTimesheetTierView> ToBatchedTimesheetTierViews(List<BatchedTimesheetTier> items)
    {
        var views = new List<BatchedTimesheetTierView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTimesheetTierView(item));
        }
        return views;
    }

    public static double SumBatchedTimesheetTierView(BatchedTimesheetTierView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedMaximum + parsedScore;
    }
}
