namespace Acme.Workforce.Compensation.PayBand.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pay band batch.</summary>
public sealed record FallbackPayBandBatchView(string Code, string Title, string Attempts, string JobCode, string Cost, string Active, string Score, string Checksum, string Budget, string Threshold, string StatusLabel)
{
    public static FallbackPayBandBatchView ToFallbackPayBandBatchView(FallbackPayBandBatch item)
    {
        return new FallbackPayBandBatchView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayBandBatchView> ToFallbackPayBandBatchViews(List<FallbackPayBandBatch> items)
    {
        var views = new List<FallbackPayBandBatchView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayBandBatchView(item));
        }
        return views;
    }

    public static double SumFallbackPayBandBatchView(FallbackPayBandBatchView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedCost + parsedScore;
    }
}
