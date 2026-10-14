namespace Acme.Workforce.Assets.Warranty.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a warranty target.</summary>
public sealed record WarrantyTargetView(string CodeText, string LabelText, string RevisionText, string Category, string ActiveText, string Quantity, string TenureMonths, string ExternalRefText, string LeaveDays, string RatingText, string StatusLabel)
{
    public static WarrantyTargetView ToWarrantyTargetView(WarrantyTarget item)
    {
        return new WarrantyTargetView(
            item.Code,
            item.Label,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            $"{item.Active}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WarrantyTargetView> ToWarrantyTargetViews(List<WarrantyTarget> items)
    {
        var views = new List<WarrantyTargetView>();
        foreach (var item in items)
        {
            views.Add(ToWarrantyTargetView(item));
        }
        return views;
    }

    public static double SumWarrantyTargetView(WarrantyTargetView view)
    {
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedQuantity + (double)parsedTenureMonths;
    }
}
