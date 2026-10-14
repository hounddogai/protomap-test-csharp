namespace Acme.Workforce.Engagement.ActionPlan.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan allocation.</summary>
public sealed record ActionPlanAllocationView(string CodeText, string LabelText, string Channel, string AmountText, string HoursText, string StatusLabel)
{
    public static ActionPlanAllocationView ToActionPlanAllocationView(ActionPlanAllocation item)
    {
        return new ActionPlanAllocationView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Channel, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanAllocationView> ToActionPlanAllocationViews(List<ActionPlanAllocation> items)
    {
        var views = new List<ActionPlanAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanAllocationView(item));
        }
        return views;
    }

    public static double SumActionPlanAllocationView(ActionPlanAllocationView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        return parsedAmount + parsedHours;
    }
}
