namespace Acme.Workforce.Assets.Refresh.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a pending refresh quota.</summary>
public sealed record PendingRefreshQuotaView(string Code, string Title, string Minimum, string Summary, string PriorityText, string BudgetText, string StartsAtText, string Timezone, string BatchSizeText, string StatusLabel)
{
    public static PendingRefreshQuotaView ToPendingRefreshQuotaView(PendingRefreshQuota item)
    {
        return new PendingRefreshQuotaView(
            item.Code,
            item.Title,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingRefreshQuotaView> ToPendingRefreshQuotaViews(List<PendingRefreshQuota> items)
    {
        var views = new List<PendingRefreshQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPendingRefreshQuotaView(item));
        }
        return views;
    }

    public static double SumPendingRefreshQuotaView(PendingRefreshQuotaView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedPriority + parsedBudget;
    }
}
