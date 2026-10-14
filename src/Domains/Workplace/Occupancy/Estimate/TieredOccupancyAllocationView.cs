namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered occupancy allocation.</summary>
public sealed record TieredOccupancyAllocationView(string CodeText, string Name, string ContactEmailText, string ManagerIdText, string BadgeText, string QuantityText, string StatusLabel)
{
    public static TieredOccupancyAllocationView ToTieredOccupancyAllocationView(TieredOccupancyAllocation item)
    {
        return new TieredOccupancyAllocationView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.ContactEmail,
            item.ManagerId,
            item.Badge,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredOccupancyAllocationView> ToTieredOccupancyAllocationViews(List<TieredOccupancyAllocation> items)
    {
        var views = new List<TieredOccupancyAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToTieredOccupancyAllocationView(item));
        }
        return views;
    }

    public static double SumTieredOccupancyAllocationView(TieredOccupancyAllocationView view)
    {
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
