namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched refresh assignment.</summary>
public sealed record BatchedRefreshAssignmentView(string Code, string NameText, string BatchSize, string CurrencyText, string WindowDaysText, string Retries, string CategoryText, string Locale, string StatusLabel)
{
    public static BatchedRefreshAssignmentView ToBatchedRefreshAssignmentView(BatchedRefreshAssignment item)
    {
        return new BatchedRefreshAssignmentView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 40),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRefreshAssignmentView> ToBatchedRefreshAssignmentViews(List<BatchedRefreshAssignment> items)
    {
        var views = new List<BatchedRefreshAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRefreshAssignmentView(item));
        }
        return views;
    }

    public static double SumBatchedRefreshAssignmentView(BatchedRefreshAssignmentView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedWindowDays + (double)parsedRetries;
    }
}
