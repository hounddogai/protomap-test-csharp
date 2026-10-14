namespace Acme.Workforce.Workplace.Desk.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback desk snapshot.</summary>
public sealed record FallbackDeskSnapshotView(string Code, string TitleText, string CreatedAt, string Notes, string RateText, string ExternalRef, string Summary, string StatusLabel)
{
    public static FallbackDeskSnapshotView ToFallbackDeskSnapshotView(FallbackDeskSnapshot item)
    {
        return new FallbackDeskSnapshotView(
            item.Code,
            item.Title,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackDeskSnapshotView> ToFallbackDeskSnapshotViews(List<FallbackDeskSnapshot> items)
    {
        var views = new List<FallbackDeskSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackDeskSnapshotView(item));
        }
        return views;
    }

    public static double SumFallbackDeskSnapshotView(FallbackDeskSnapshotView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedRate;
    }
}
