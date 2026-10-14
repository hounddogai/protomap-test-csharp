namespace Acme.Workforce.Compensation.PayBand.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pay band snapshot.</summary>
public sealed record FallbackPayBandSnapshotView(string Code, string Title, string Archived, string Version, string ManagerIdText, string CurrencyText, string Rate, string WeightText, string StatusLabel)
{
    public static FallbackPayBandSnapshotView ToFallbackPayBandSnapshotView(FallbackPayBandSnapshot item)
    {
        return new FallbackPayBandSnapshotView(
            Text.Truncate(item.Code, 64),
            item.Title,
            $"{item.Archived}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 80),
            item.Currency,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayBandSnapshotView> ToFallbackPayBandSnapshotViews(List<FallbackPayBandSnapshot> items)
    {
        var views = new List<FallbackPayBandSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayBandSnapshotView(item));
        }
        return views;
    }

    public static double SumFallbackPayBandSnapshotView(FallbackPayBandSnapshotView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedRate + parsedWeight;
    }
}
