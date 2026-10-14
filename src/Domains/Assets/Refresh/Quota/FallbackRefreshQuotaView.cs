namespace Acme.Workforce.Assets.Refresh.Quota;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback refresh quota.</summary>
public sealed record FallbackRefreshQuotaView(string Code, string Name, string Locale, string ManagerId, string Amount, string SourceText, string BatchSizeText, string ExternalRef, string Hours, string MaximumText, string StatusLabel)
{
    public static FallbackRefreshQuotaView ToFallbackRefreshQuotaView(FallbackRefreshQuota item)
    {
        return new FallbackRefreshQuotaView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Locale, 64),
            item.ManagerId,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackRefreshQuotaView> ToFallbackRefreshQuotaViews(List<FallbackRefreshQuota> items)
    {
        var views = new List<FallbackRefreshQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackRefreshQuotaView(item));
        }
        return views;
    }

    public static double SumFallbackRefreshQuotaView(FallbackRefreshQuotaView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedBatchSize + parsedHours;
    }
}
