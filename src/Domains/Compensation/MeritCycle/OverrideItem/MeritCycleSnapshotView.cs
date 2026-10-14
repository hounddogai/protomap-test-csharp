namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle snapshot.</summary>
public sealed record MeritCycleSnapshotView(string Code, string LabelText, string TenureMonthsText, string Version, string Region, string Capacity, string CreatedAt, string StatusLabel)
{
    public static MeritCycleSnapshotView ToMeritCycleSnapshotView(MeritCycleSnapshot item)
    {
        return new MeritCycleSnapshotView(
            item.Code,
            Text.Truncate(item.Label, 64),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Region,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCycleSnapshotView> ToMeritCycleSnapshotViews(List<MeritCycleSnapshot> items)
    {
        var views = new List<MeritCycleSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCycleSnapshotView(item));
        }
        return views;
    }

    public static double SumMeritCycleSnapshotView(MeritCycleSnapshotView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedVersion + (double)parsedCapacity;
    }
}
