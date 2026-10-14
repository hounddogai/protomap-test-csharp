namespace Acme.Workforce.Engagement.TownHall.HallTier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive town hall rule.</summary>
public sealed record AdaptiveTownHallRuleView(string Code, string Name, string Revision, string Ceiling, string Visible, string DepartmentText, string FloorValueText, string CreatedAtText, string StatusLabel)
{
    public static AdaptiveTownHallRuleView ToAdaptiveTownHallRuleView(AdaptiveTownHallRule item)
    {
        return new AdaptiveTownHallRuleView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            Text.Truncate(item.Department, 120),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTownHallRuleView> ToAdaptiveTownHallRuleViews(List<AdaptiveTownHallRule> items)
    {
        var views = new List<AdaptiveTownHallRuleView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTownHallRuleView(item));
        }
        return views;
    }

    public static double SumAdaptiveTownHallRuleView(AdaptiveTownHallRuleView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedCeiling + parsedFloorValue;
    }
}
