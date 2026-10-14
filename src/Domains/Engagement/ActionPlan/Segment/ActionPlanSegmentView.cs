namespace Acme.Workforce.Engagement.ActionPlan.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan segment.</summary>
public sealed record ActionPlanSegmentView(string Code, string TitleText, string Channel, string Timezone, string DiscountText, string StatusLabel)
{
    public static ActionPlanSegmentView ToActionPlanSegmentView(ActionPlanSegment item)
    {
        return new ActionPlanSegmentView(
            item.Code,
            item.Title,
            Text.Truncate(item.Channel, 120),
            item.Timezone,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanSegmentView> ToActionPlanSegmentViews(List<ActionPlanSegment> items)
    {
        var views = new List<ActionPlanSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanSegmentView(item));
        }
        return views;
    }

    public static double SumActionPlanSegmentView(ActionPlanSegmentView view)
    {
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return parsedDiscount;
    }
}
