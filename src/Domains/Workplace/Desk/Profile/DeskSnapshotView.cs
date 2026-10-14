namespace Acme.Workforce.Workplace.Desk.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a desk snapshot.</summary>
public sealed record DeskSnapshotView(string CodeText, string LabelText, string Limit, string Cost, string Name, string Maximum, string Currency, string Sequence, string Revision, string Capacity, string StatusLabel)
{
    public static DeskSnapshotView ToDeskSnapshotView(DeskSnapshot item)
    {
        return new DeskSnapshotView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Currency,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeskSnapshotView> ToDeskSnapshotViews(List<DeskSnapshot> items)
    {
        var views = new List<DeskSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToDeskSnapshotView(item));
        }
        return views;
    }

    public static double SumDeskSnapshotView(DeskSnapshotView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedCost + parsedMaximum;
    }
}
