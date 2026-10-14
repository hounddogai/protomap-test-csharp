namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan cycle.</summary>
public sealed record ActionPlanCycleView(string Code, string NameText, string TeamText, string TenureMonths, string ContactEmailText, string OwnerIdText, string StatusLabel)
{
    public static ActionPlanCycleView ToActionPlanCycleView(ActionPlanCycle item)
    {
        return new ActionPlanCycleView(
            item.Code,
            item.Name,
            item.Team,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            Text.Truncate(item.OwnerId, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanCycleView> ToActionPlanCycleViews(List<ActionPlanCycle> items)
    {
        var views = new List<ActionPlanCycleView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanCycleView(item));
        }
        return views;
    }

    public static double SumActionPlanCycleView(ActionPlanCycleView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
