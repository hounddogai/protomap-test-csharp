namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional access badge score.</summary>
public sealed record RegionalAccessBadgeScoreView(string CodeText, string Name, string ContactEmail, string Capacity, string FloorValueText, string OwnerId, string LeaveDays, string DescriptionText, string Score, string Skill, string WindowDaysText, string StatusLabel)
{
    public static RegionalAccessBadgeScoreView ToRegionalAccessBadgeScoreView(RegionalAccessBadgeScore item)
    {
        return new RegionalAccessBadgeScoreView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.ContactEmail,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.OwnerId,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalAccessBadgeScoreView> ToRegionalAccessBadgeScoreViews(List<RegionalAccessBadgeScore> items)
    {
        var views = new List<RegionalAccessBadgeScoreView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalAccessBadgeScoreView(item));
        }
        return views;
    }

    public static double SumRegionalAccessBadgeScoreView(RegionalAccessBadgeScoreView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFloorValue + (double)parsedLeaveDays;
    }
}
