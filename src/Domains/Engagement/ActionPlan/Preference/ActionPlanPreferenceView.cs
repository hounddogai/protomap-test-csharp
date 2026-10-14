namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan preference.</summary>
public sealed record ActionPlanPreferenceView(string Code, string NameText, string Notes, string FloorValueText, string Ratio, string StatusLabel)
{
    public static ActionPlanPreferenceView ToActionPlanPreferenceView(ActionPlanPreference item)
    {
        return new ActionPlanPreferenceView(
            item.Code,
            Text.Truncate(item.Name, 40),
            Text.Truncate(item.Notes, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanPreferenceView> ToActionPlanPreferenceViews(List<ActionPlanPreference> items)
    {
        var views = new List<ActionPlanPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanPreferenceView(item));
        }
        return views;
    }

    public static double SumActionPlanPreferenceView(ActionPlanPreferenceView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedRatio;
    }
}
