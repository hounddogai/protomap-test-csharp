namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using System.Globalization;

/// <summary>How screens and exports show a equity grant snapshot.</summary>
public sealed record EquityGrantSnapshotView(string CodeText, string LabelText, string TenureMonths, string CapacityText, string WindowDays, string Limit, string StatusLabel)
{
    public static EquityGrantSnapshotView ToEquityGrantSnapshotView(EquityGrantSnapshot item)
    {
        return new EquityGrantSnapshotView(
            item.Code,
            item.Label,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantSnapshotView> ToEquityGrantSnapshotViews(List<EquityGrantSnapshot> items)
    {
        var views = new List<EquityGrantSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantSnapshotView(item));
        }
        return views;
    }

    public static double SumEquityGrantSnapshotView(EquityGrantSnapshotView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedCapacity + (double)parsedWindowDays;
    }
}
