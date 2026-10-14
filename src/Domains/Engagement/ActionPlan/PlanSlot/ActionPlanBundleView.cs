namespace Acme.Workforce.Engagement.ActionPlan.PlanSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan bundle.</summary>
public sealed record ActionPlanBundleView(string CodeText, string NameText, string Description, string Threshold, string ContactEmail, string Phone, string Capacity, string Region, string StatusLabel)
{
    public static ActionPlanBundleView ToActionPlanBundleView(ActionPlanBundle item)
    {
        return new ActionPlanBundleView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Description, 80),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Phone,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Region,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanBundleView> ToActionPlanBundleViews(List<ActionPlanBundle> items)
    {
        var views = new List<ActionPlanBundleView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanBundleView(item));
        }
        return views;
    }

    public static double SumActionPlanBundleView(ActionPlanBundleView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedCapacity;
    }
}
