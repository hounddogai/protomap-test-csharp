namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using System.Globalization;

/// <summary>How screens and exports show a rolling accrual window.</summary>
public sealed record RollingAccrualWindowView(string CodeText, string Title, string Cost, string DurationMinutes, string Unit, string ManagerId, string StatusLabel)
{
    public static RollingAccrualWindowView ToRollingAccrualWindowView(RollingAccrualWindow item)
    {
        return new RollingAccrualWindowView(
            item.Code,
            item.Title,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingAccrualWindowView> ToRollingAccrualWindowViews(List<RollingAccrualWindow> items)
    {
        var views = new List<RollingAccrualWindowView>();
        foreach (var item in items)
        {
            views.Add(ToRollingAccrualWindowView(item));
        }
        return views;
    }

    public static double SumRollingAccrualWindowView(RollingAccrualWindowView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedDurationMinutes;
    }
}
