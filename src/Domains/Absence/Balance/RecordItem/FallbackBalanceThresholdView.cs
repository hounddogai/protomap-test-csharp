namespace Acme.Workforce.Absence.Balance.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback balance threshold.</summary>
public sealed record FallbackBalanceThresholdView(string Code, string Title, string RetriesText, string BatchSize, string Fte, string ChecksumText, string Category, string StatusLabel)
{
    public static FallbackBalanceThresholdView ToFallbackBalanceThresholdView(FallbackBalanceThreshold item)
    {
        return new FallbackBalanceThresholdView(
            item.Code,
            item.Title,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.Category, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackBalanceThresholdView> ToFallbackBalanceThresholdViews(List<FallbackBalanceThreshold> items)
    {
        var views = new List<FallbackBalanceThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackBalanceThresholdView(item));
        }
        return views;
    }

    public static double SumFallbackBalanceThresholdView(FallbackBalanceThresholdView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedBatchSize + parsedFte;
    }
}
