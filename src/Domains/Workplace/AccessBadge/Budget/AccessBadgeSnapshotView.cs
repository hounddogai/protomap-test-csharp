namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a access badge snapshot.</summary>
public sealed record AccessBadgeSnapshotView(string Code, string Label, string Capacity, string OwnerId, string Reviewer, string RevisionText, string Cost, string UpdatedAtText, string Discount, string StatusLabel)
{
    public static AccessBadgeSnapshotView ToAccessBadgeSnapshotView(AccessBadgeSnapshot item)
    {
        return new AccessBadgeSnapshotView(
            item.Code,
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            Text.Truncate(item.Reviewer, 64),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccessBadgeSnapshotView> ToAccessBadgeSnapshotViews(List<AccessBadgeSnapshot> items)
    {
        var views = new List<AccessBadgeSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToAccessBadgeSnapshotView(item));
        }
        return views;
    }

    public static double SumAccessBadgeSnapshotView(AccessBadgeSnapshotView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedRevision + parsedCost;
    }
}
