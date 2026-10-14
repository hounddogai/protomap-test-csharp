namespace Acme.Workforce.Workplace.Occupancy.Target;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a occupancy target.</summary>
public sealed record OccupancyTargetView(string Code, string NameText, string Fte, string HoursText, string ContactEmail, string JobCode, string Location, string RatioText, string StatusLabel)
{
    public static OccupancyTargetView ToOccupancyTargetView(OccupancyTarget item)
    {
        return new OccupancyTargetView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 64),
            item.JobCode,
            item.Location,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyTargetView> ToOccupancyTargetViews(List<OccupancyTarget> items)
    {
        var views = new List<OccupancyTargetView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyTargetView(item));
        }
        return views;
    }

    public static double SumOccupancyTargetView(OccupancyTargetView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return parsedFte + parsedHours + parsedRatio;
    }
}
