namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan request.</summary>
public sealed record ActionPlanRequestView(string CodeText, string Name, string Version, string StartsAt, string Department, string Attempts, string Visible, string StatusLabel)
{
    public static ActionPlanRequestView ToActionPlanRequestView(ActionPlanRequest item)
    {
        return new ActionPlanRequestView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanRequestView> ToActionPlanRequestViews(List<ActionPlanRequest> items)
    {
        var views = new List<ActionPlanRequestView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanRequestView(item));
        }
        return views;
    }

    public static double SumActionPlanRequestView(ActionPlanRequestView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedStartsAt + (double)parsedAttempts;
    }
}
