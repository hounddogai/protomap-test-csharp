namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy tier.</summary>
public sealed record OccupancyTierView(string Code, string Title, string BadgeText, string LocaleText, string CostCenter, string Revision, string WindowDays, string Unit, string StatusLabel)
{
    public static OccupancyTierView ToOccupancyTierView(OccupancyTier item)
    {
        return new OccupancyTierView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 80),
            item.Badge,
            item.Locale,
            item.CostCenter,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyTierView> ToOccupancyTierViews(List<OccupancyTier> items)
    {
        var views = new List<OccupancyTierView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyTierView(item));
        }
        return views;
    }

    public static double SumOccupancyTierView(OccupancyTierView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedRevision + (double)parsedWindowDays;
    }
}
