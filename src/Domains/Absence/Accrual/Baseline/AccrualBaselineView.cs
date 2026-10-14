namespace Acme.Workforce.Absence.Accrual.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual baseline.</summary>
public sealed record AccrualBaselineView(string CodeText, string Title, string BatchSize, string DurationMinutesText, string OwnerIdText, string StatusLabel)
{
    public static AccrualBaselineView ToAccrualBaselineView(AccrualBaseline item)
    {
        return new AccrualBaselineView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualBaselineView> ToAccrualBaselineViews(List<AccrualBaseline> items)
    {
        var views = new List<AccrualBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualBaselineView(item));
        }
        return views;
    }

    public static double SumAccrualBaselineView(AccrualBaselineView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedDurationMinutes;
    }
}
