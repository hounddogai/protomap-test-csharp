namespace Acme.Workforce.Engagement.TownHall.Tier;

using System.Globalization;

/// <summary>How screens and exports show a town hall plan.</summary>
public sealed record TownHallPlanView(string Code, string NameText, string Unit, string Maximum, string ArchivedText, string EndsAt, string ExternalRef, string DurationMinutes, string Capacity, string ApprovedText, string StatusLabel)
{
    public static TownHallPlanView ToTownHallPlanView(TownHallPlan item)
    {
        return new TownHallPlanView(
            item.Code,
            item.Name,
            item.Unit,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallPlanView> ToTownHallPlanViews(List<TownHallPlan> items)
    {
        var views = new List<TownHallPlanView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallPlanView(item));
        }
        return views;
    }

    public static double SumTownHallPlanView(TownHallPlanView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedEndsAt + (double)parsedDurationMinutes;
    }
}
