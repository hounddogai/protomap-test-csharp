namespace Acme.Workforce.Contractors.Timesheet.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched timesheet estimate.</summary>
public sealed record BatchedTimesheetEstimateView(string CodeText, string Title, string Rating, string Notes, string ManagerIdText, string ContactEmail, string StatusLabel)
{
    public static BatchedTimesheetEstimateView ToBatchedTimesheetEstimateView(BatchedTimesheetEstimate item)
    {
        return new BatchedTimesheetEstimateView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Notes,
            item.ManagerId,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTimesheetEstimateView> ToBatchedTimesheetEstimateViews(List<BatchedTimesheetEstimate> items)
    {
        var views = new List<BatchedTimesheetEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTimesheetEstimateView(item));
        }
        return views;
    }

    public static double SumBatchedTimesheetEstimateView(BatchedTimesheetEstimateView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        return parsedRating;
    }
}
