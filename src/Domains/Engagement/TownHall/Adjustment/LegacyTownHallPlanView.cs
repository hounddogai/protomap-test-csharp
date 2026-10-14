namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy town hall plan.</summary>
public sealed record LegacyTownHallPlanView(string Code, string NameText, string ManagerId, string Archived, string Checksum, string Revision, string StatusLabel)
{
    public static LegacyTownHallPlanView ToLegacyTownHallPlanView(LegacyTownHallPlan item)
    {
        return new LegacyTownHallPlanView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.ManagerId,
            $"{item.Archived}",
            item.Checksum,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyTownHallPlanView> ToLegacyTownHallPlanViews(List<LegacyTownHallPlan> items)
    {
        var views = new List<LegacyTownHallPlanView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyTownHallPlanView(item));
        }
        return views;
    }

    public static double SumLegacyTownHallPlanView(LegacyTownHallPlanView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedRevision;
    }
}
