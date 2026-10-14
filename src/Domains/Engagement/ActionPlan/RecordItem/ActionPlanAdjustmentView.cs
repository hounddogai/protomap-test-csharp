namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan adjustment.</summary>
public sealed record ActionPlanAdjustmentView(string Code, string Title, string GraceDays, string Ratio, string Country, string DiscountText, string TimezoneText, string OwnerId, string EndsAt, string Reviewer, string ManagerId, string StatusLabel)
{
    public static ActionPlanAdjustmentView ToActionPlanAdjustmentView(ActionPlanAdjustment item)
    {
        return new ActionPlanAdjustmentView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            item.OwnerId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanAdjustmentView> ToActionPlanAdjustmentViews(List<ActionPlanAdjustment> items)
    {
        var views = new List<ActionPlanAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanAdjustmentView(item));
        }
        return views;
    }

    public static double SumActionPlanAdjustmentView(ActionPlanAdjustmentView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.DiscountText, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedRatio + parsedDiscount;
    }
}
