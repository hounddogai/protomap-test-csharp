namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy manifest.</summary>
public sealed record OccupancyManifestView(string Code, string NameText, string Capacity, string DurationMinutes, string SalaryBand, string Category, string Amount, string RegionText, string StatusLabel)
{
    public static OccupancyManifestView ToOccupancyManifestView(OccupancyManifest item)
    {
        return new OccupancyManifestView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyManifestView> ToOccupancyManifestViews(List<OccupancyManifest> items)
    {
        var views = new List<OccupancyManifestView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyManifestView(item));
        }
        return views;
    }

    public static double SumOccupancyManifestView(OccupancyManifestView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedDurationMinutes + parsedAmount;
    }
}
