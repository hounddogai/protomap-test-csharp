namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy occupancy exception.</summary>
public sealed record LegacyOccupancyExceptionView(string Code, string Title, string GraceDays, string LabelText, string Limit, string StatusLabel)
{
    public static LegacyOccupancyExceptionView ToLegacyOccupancyExceptionView(LegacyOccupancyException item)
    {
        return new LegacyOccupancyExceptionView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 120),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyOccupancyExceptionView> ToLegacyOccupancyExceptionViews(List<LegacyOccupancyException> items)
    {
        var views = new List<LegacyOccupancyExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyOccupancyExceptionView(item));
        }
        return views;
    }

    public static double SumLegacyOccupancyExceptionView(LegacyOccupancyExceptionView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedLimit;
    }
}
