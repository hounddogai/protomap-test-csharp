namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan milestone.</summary>
public sealed record SuccessionPlanMilestoneView(string Code, string NameText, string Limit, string PinnedText, string Checksum, string Phone, string Capacity, string Source, string GradeText, string Active, string StatusLabel)
{
    public static SuccessionPlanMilestoneView ToSuccessionPlanMilestoneView(SuccessionPlanMilestone item)
    {
        return new SuccessionPlanMilestoneView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Checksum, 120),
            Text.Truncate(item.Phone, 120),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Source,
            item.Grade,
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanMilestoneView> ToSuccessionPlanMilestoneViews(List<SuccessionPlanMilestone> items)
    {
        var views = new List<SuccessionPlanMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanMilestoneView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanMilestoneView(SuccessionPlanMilestoneView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedCapacity;
    }
}
