namespace Acme.Workforce.Engagement.ActionPlan.Group;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan group.</summary>
public sealed record ActionPlanGroupView(string Code, string Label, string FloorValueText, string Team, string Summary, string RatingText, string CategoryText, string LeaveDaysText, string StatusLabel)
{
    public static ActionPlanGroupView ToActionPlanGroupView(ActionPlanGroup item)
    {
        return new ActionPlanGroupView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Summary,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Category,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanGroupView> ToActionPlanGroupViews(List<ActionPlanGroup> items)
    {
        var views = new List<ActionPlanGroupView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanGroupView(item));
        }
        return views;
    }

    public static double SumActionPlanGroupView(ActionPlanGroupView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRating + (double)parsedLeaveDays;
    }
}
