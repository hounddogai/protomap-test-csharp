namespace Acme.Workforce.Engagement.ActionPlan.RegionalPlanPreference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan variance.</summary>
public sealed record ActionPlanVarianceView(string CodeText, string LabelText, string Name, string OwnerId, string EmployeeId, string CreatedAt, string Capacity, string StatusLabel)
{
    public static ActionPlanVarianceView ToActionPlanVarianceView(ActionPlanVariance item)
    {
        return new ActionPlanVarianceView(
            item.Code,
            item.Label,
            item.Name,
            Text.Truncate(item.OwnerId, 120),
            item.EmployeeId,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanVarianceView> ToActionPlanVarianceViews(List<ActionPlanVariance> items)
    {
        var views = new List<ActionPlanVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanVarianceView(item));
        }
        return views;
    }

    public static double SumActionPlanVarianceView(ActionPlanVarianceView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedCapacity;
    }
}
