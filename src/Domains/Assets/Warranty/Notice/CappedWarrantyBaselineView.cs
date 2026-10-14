namespace Acme.Workforce.Assets.Warranty.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped warranty baseline.</summary>
public sealed record CappedWarrantyBaselineView(string Code, string Label, string CategoryText, string Notes, string Region, string RevisionText, string StartsAt, string Limit, string Quantity, string StatusLabel)
{
    public static CappedWarrantyBaselineView ToCappedWarrantyBaselineView(CappedWarrantyBaseline item)
    {
        return new CappedWarrantyBaselineView(
            item.Code,
            item.Label,
            item.Category,
            item.Notes,
            Text.Truncate(item.Region, 120),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedWarrantyBaselineView> ToCappedWarrantyBaselineViews(List<CappedWarrantyBaseline> items)
    {
        var views = new List<CappedWarrantyBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToCappedWarrantyBaselineView(item));
        }
        return views;
    }

    public static double SumCappedWarrantyBaselineView(CappedWarrantyBaselineView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedStartsAt + (double)parsedLimit;
    }
}
