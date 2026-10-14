namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive bonus pool baseline.</summary>
public sealed record AdaptiveBonusPoolBaselineView(string Code, string LabelText, string Visible, string SkillText, string CapacityText, string StatusLabel)
{
    public static AdaptiveBonusPoolBaselineView ToAdaptiveBonusPoolBaselineView(AdaptiveBonusPoolBaseline item)
    {
        return new AdaptiveBonusPoolBaselineView(
            Text.Truncate(item.Code, 64),
            item.Label,
            $"{item.Visible}",
            item.Skill,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveBonusPoolBaselineView> ToAdaptiveBonusPoolBaselineViews(List<AdaptiveBonusPoolBaseline> items)
    {
        var views = new List<AdaptiveBonusPoolBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveBonusPoolBaselineView(item));
        }
        return views;
    }

    public static double SumAdaptiveBonusPoolBaselineView(AdaptiveBonusPoolBaselineView view)
    {
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity;
    }
}
