namespace Acme.Workforce.Payrollops.Garnishment.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment snapshot.</summary>
public sealed record GarnishmentSnapshotView(string Code, string Title, string EndsAt, string Revision, string Region, string StatusLabel)
{
    public static GarnishmentSnapshotView ToGarnishmentSnapshotView(GarnishmentSnapshot item)
    {
        return new GarnishmentSnapshotView(
            item.Code,
            item.Title,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentSnapshotView> ToGarnishmentSnapshotViews(List<GarnishmentSnapshot> items)
    {
        var views = new List<GarnishmentSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentSnapshotView(item));
        }
        return views;
    }

    public static double SumGarnishmentSnapshotView(GarnishmentSnapshotView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedRevision;
    }
}
