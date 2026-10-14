namespace Acme.Workforce.Engagement.TownHall.HallTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a town hall assignment.</summary>
public sealed record TownHallAssignmentView(string Code, string Title, string Limit, string WindowDaysText, string CountryText, string Active, string Attempts, string StatusLabel)
{
    public static TownHallAssignmentView ToTownHallAssignmentView(TownHallAssignment item)
    {
        return new TownHallAssignmentView(
            item.Code,
            item.Title,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 40),
            $"{item.Active}",
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TownHallAssignmentView> ToTownHallAssignmentViews(List<TownHallAssignment> items)
    {
        var views = new List<TownHallAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToTownHallAssignmentView(item));
        }
        return views;
    }

    public static double SumTownHallAssignmentView(TownHallAssignmentView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedWindowDays + (double)parsedAttempts;
    }
}
