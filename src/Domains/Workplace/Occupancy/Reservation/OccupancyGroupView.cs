namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy group.</summary>
public sealed record OccupancyGroupView(string CodeText, string NameText, string Margin, string TenureMonths, string ManagerId, string StartsAtText, string Ratio, string CostCenterText, string JobCode, string Location, string StatusLabel)
{
    public static OccupancyGroupView ToOccupancyGroupView(OccupancyGroup item)
    {
        return new OccupancyGroupView(
            item.Code,
            item.Name,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ManagerId, 64),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.JobCode,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyGroupView> ToOccupancyGroupViews(List<OccupancyGroup> items)
    {
        var views = new List<OccupancyGroupView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyGroupView(item));
        }
        return views;
    }

    public static double SumOccupancyGroupView(OccupancyGroupView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedTenureMonths + (double)parsedStartsAt;
    }
}
