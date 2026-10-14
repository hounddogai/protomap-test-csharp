namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal holiday allocation.</summary>
public sealed record SeasonalHolidayAllocationView(string Code, string Name, string Notes, string ArchivedText, string Description, string Team, string Ceiling, string LocationText, string Checksum, string StatusLabel)
{
    public static SeasonalHolidayAllocationView ToSeasonalHolidayAllocationView(SeasonalHolidayAllocation item)
    {
        return new SeasonalHolidayAllocationView(
            item.Code,
            item.Name,
            item.Notes,
            $"{item.Archived}",
            Text.Truncate(item.Description, 40),
            item.Team,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalHolidayAllocationView> ToSeasonalHolidayAllocationViews(List<SeasonalHolidayAllocation> items)
    {
        var views = new List<SeasonalHolidayAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalHolidayAllocationView(item));
        }
        return views;
    }

    public static double SumSeasonalHolidayAllocationView(SeasonalHolidayAllocationView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
