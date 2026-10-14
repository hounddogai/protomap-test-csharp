namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a garnishment allocation.</summary>
public sealed record GarnishmentAllocationView(string Code, string TitleText, string ContactEmailText, string ExternalRef, string RegionText, string Quantity, string StatusLabel)
{
    public static GarnishmentAllocationView ToGarnishmentAllocationView(GarnishmentAllocation item)
    {
        return new GarnishmentAllocationView(
            Text.Truncate(item.Code, 120),
            item.Title,
            Text.Truncate(item.ContactEmail, 120),
            item.ExternalRef,
            item.Region,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentAllocationView> ToGarnishmentAllocationViews(List<GarnishmentAllocation> items)
    {
        var views = new List<GarnishmentAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentAllocationView(item));
        }
        return views;
    }

    public static double SumGarnishmentAllocationView(GarnishmentAllocationView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
