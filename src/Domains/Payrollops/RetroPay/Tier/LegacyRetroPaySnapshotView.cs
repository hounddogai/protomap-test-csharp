namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy retro pay snapshot.</summary>
public sealed record LegacyRetroPaySnapshotView(string CodeText, string TitleText, string BadgeText, string CreatedAtText, string Checksum, string MinimumText, string CapacityText, string Amount, string StatusLabel)
{
    public static LegacyRetroPaySnapshotView ToLegacyRetroPaySnapshotView(LegacyRetroPaySnapshot item)
    {
        return new LegacyRetroPaySnapshotView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Badge,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyRetroPaySnapshotView> ToLegacyRetroPaySnapshotViews(List<LegacyRetroPaySnapshot> items)
    {
        var views = new List<LegacyRetroPaySnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyRetroPaySnapshotView(item));
        }
        return views;
    }

    public static double SumLegacyRetroPaySnapshotView(LegacyRetroPaySnapshotView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedMinimum + (double)parsedCapacity;
    }
}
