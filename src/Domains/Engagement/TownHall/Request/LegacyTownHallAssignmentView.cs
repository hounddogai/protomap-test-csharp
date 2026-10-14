namespace Acme.Workforce.Engagement.TownHall.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy town hall assignment.</summary>
public sealed record LegacyTownHallAssignmentView(string CodeText, string Label, string Grade, string BadgeText, string Limit, string StartsAt, string EndsAt, string ExpiresAt, string EmployeeId, string Score, string LocationText, string StatusLabel)
{
    public static LegacyTownHallAssignmentView ToLegacyTownHallAssignmentView(LegacyTownHallAssignment item)
    {
        return new LegacyTownHallAssignmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.Grade, 40),
            item.Badge,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTownHallAssignmentView> ToLegacyTownHallAssignmentViews(List<LegacyTownHallAssignment> items)
    {
        var views = new List<LegacyTownHallAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTownHallAssignmentView(item));
        }
        return views;
    }

    public static double SumLegacyTownHallAssignmentView(LegacyTownHallAssignmentView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedStartsAt + (double)parsedEndsAt;
    }
}
