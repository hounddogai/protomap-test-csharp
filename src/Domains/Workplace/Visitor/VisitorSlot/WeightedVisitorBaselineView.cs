namespace Acme.Workforce.Workplace.Visitor.VisitorSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted visitor baseline.</summary>
public sealed record WeightedVisitorBaselineView(string Code, string Label, string OwnerId, string Segment, string SkillText, string Version, string Revision, string Minimum, string DescriptionText, string Discount, string Region, string StatusLabel)
{
    public static WeightedVisitorBaselineView ToWeightedVisitorBaselineView(WeightedVisitorBaseline item)
    {
        return new WeightedVisitorBaselineView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.OwnerId,
            item.Segment,
            Text.Truncate(item.Skill, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedVisitorBaselineView> ToWeightedVisitorBaselineViews(List<WeightedVisitorBaseline> items)
    {
        var views = new List<WeightedVisitorBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedVisitorBaselineView(item));
        }
        return views;
    }

    public static double SumWeightedVisitorBaselineView(WeightedVisitorBaselineView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedRevision + parsedMinimum;
    }
}
