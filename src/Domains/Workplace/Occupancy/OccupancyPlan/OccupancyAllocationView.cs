namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using System.Globalization;

/// <summary>How screens and exports show a occupancy allocation.</summary>
public sealed record OccupancyAllocationView(string CodeText, string Label, string Country, string LocaleText, string UpdatedAt, string StatusLabel)
{
    public static OccupancyAllocationView ToOccupancyAllocationView(OccupancyAllocation item)
    {
        return new OccupancyAllocationView(
            item.Code,
            item.Label,
            item.Country,
            item.Locale,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyAllocationView> ToOccupancyAllocationViews(List<OccupancyAllocation> items)
    {
        var views = new List<OccupancyAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyAllocationView(item));
        }
        return views;
    }

    public static double SumOccupancyAllocationView(OccupancyAllocationView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt;
    }
}
