namespace Acme.Workforce.Contractors.Timesheet.Rollup;

using System.Globalization;

/// <summary>How screens and exports show a tiered timesheet profile.</summary>
public sealed record TieredTimesheetProfileView(string Code, string Title, string StartsAtText, string Attempts, string Label, string FteText, string Cost, string WindowDays, string BatchSize, string Team, string TenureMonths, string StatusLabel)
{
    public static TieredTimesheetProfileView ToTieredTimesheetProfileView(TieredTimesheetProfile item)
    {
        return new TieredTimesheetProfileView(
            item.Code,
            item.Title,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredTimesheetProfileView> ToTieredTimesheetProfileViews(List<TieredTimesheetProfile> items)
    {
        var views = new List<TieredTimesheetProfileView>();
        foreach (var item in items)
        {
            views.Add(ToTieredTimesheetProfileView(item));
        }
        return views;
    }

    public static double SumTieredTimesheetProfileView(TieredTimesheetProfileView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedAttempts + parsedFte;
    }
}
