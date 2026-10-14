namespace Acme.Workforce.Contractors.Vendor.Score;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive vendor assignment.</summary>
public sealed record AdaptiveVendorAssignmentView(string Code, string Title, string Margin, string Phone, string ExternalRef, string Limit, string Quantity, string Department, string StartsAtText, string RatioText, string StatusLabel)
{
    public static AdaptiveVendorAssignmentView ToAdaptiveVendorAssignmentView(AdaptiveVendorAssignment item)
    {
        return new AdaptiveVendorAssignmentView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.ExternalRef,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveVendorAssignmentView> ToAdaptiveVendorAssignmentViews(List<AdaptiveVendorAssignment> items)
    {
        var views = new List<AdaptiveVendorAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveVendorAssignmentView(item));
        }
        return views;
    }

    public static double SumAdaptiveVendorAssignmentView(AdaptiveVendorAssignmentView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedLimit + (double)parsedQuantity;
    }
}
