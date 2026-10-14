namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending reorg snapshot.</summary>
public sealed record PendingReorgSnapshotView(string CodeText, string Label, string Ceiling, string Margin, string CountryText, string Name, string ChannelText, string StatusLabel)
{
    public static PendingReorgSnapshotView ToPendingReorgSnapshotView(PendingReorgSnapshot item)
    {
        return new PendingReorgSnapshotView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Name,
            item.Channel,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingReorgSnapshotView> ToPendingReorgSnapshotViews(List<PendingReorgSnapshot> items)
    {
        var views = new List<PendingReorgSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToPendingReorgSnapshotView(item));
        }
        return views;
    }

    public static double SumPendingReorgSnapshotView(PendingReorgSnapshotView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMargin;
    }
}
