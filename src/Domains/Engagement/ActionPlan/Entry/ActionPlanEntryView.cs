namespace Acme.Workforce.Engagement.ActionPlan.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a action plan entry.</summary>
public sealed record ActionPlanEntryView(string Code, string LabelText, string Badge, string Segment, string Threshold, string NotesText, string ManagerIdText, string Retries, string StatusLabel)
{
    public static ActionPlanEntryView ToActionPlanEntryView(ActionPlanEntry item)
    {
        return new ActionPlanEntryView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Badge,
            item.Segment,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 80),
            item.ManagerId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ActionPlanEntryView> ToActionPlanEntryViews(List<ActionPlanEntry> items)
    {
        var views = new List<ActionPlanEntryView>();
        foreach (var item in items)
        {
            views.Add(ToActionPlanEntryView(item));
        }
        return views;
    }

    public static double SumActionPlanEntryView(ActionPlanEntryView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedRetries;
    }
}
