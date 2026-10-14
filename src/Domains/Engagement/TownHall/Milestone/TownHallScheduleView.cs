namespace Acme.Workforce.Engagement.TownHall.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall schedule.</summary>
public sealed record TownHallScheduleView(string Code, string LabelText, string Hours, string ExpiresAt, string FloorValue, string ContactEmail, string SummaryText, string Description, string StatusLabel)
{
    public static TownHallScheduleView ToTownHallScheduleView(TownHallSchedule item)
    {
        return new TownHallScheduleView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Summary,
            Text.Truncate(item.Description, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallScheduleView> ToTownHallScheduleViews(List<TownHallSchedule> items)
    {
        var views = new List<TownHallScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallScheduleView(item));
        }
        return views;
    }

    public static double SumTownHallScheduleView(TownHallScheduleView view)
    {
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedHours + (double)parsedExpiresAt + parsedFloorValue;
    }
}
