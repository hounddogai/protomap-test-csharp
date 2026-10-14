namespace Acme.Workforce.Contractors.Rate.RateProposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched rate variance.</summary>
public sealed record BatchedRateVarianceView(string Code, string Title, string Locale, string ExpiresAt, string Capacity, string StartsAt, string Attempts, string VersionText, string Cost, string StatusLabel)
{
    public static BatchedRateVarianceView ToBatchedRateVarianceView(BatchedRateVariance item)
    {
        return new BatchedRateVarianceView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.Locale,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRateVarianceView> ToBatchedRateVarianceViews(List<BatchedRateVariance> items)
    {
        var views = new List<BatchedRateVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRateVarianceView(item));
        }
        return views;
    }

    public static double SumBatchedRateVarianceView(BatchedRateVarianceView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedCapacity + (double)parsedStartsAt;
    }
}
