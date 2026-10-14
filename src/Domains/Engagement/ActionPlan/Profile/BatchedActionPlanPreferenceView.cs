namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched action plan preference.</summary>
public sealed record BatchedActionPlanPreferenceView(string Code, string Label, string Team, string Description, string NotesText, string EmployeeId, string TenureMonths, string StatusLabel)
{
    public static BatchedActionPlanPreferenceView ToBatchedActionPlanPreferenceView(BatchedActionPlanPreference item)
    {
        return new BatchedActionPlanPreferenceView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Team, 80),
            item.Description,
            item.Notes,
            Text.Truncate(item.EmployeeId, 80),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedActionPlanPreferenceView> ToBatchedActionPlanPreferenceViews(List<BatchedActionPlanPreference> items)
    {
        var views = new List<BatchedActionPlanPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedActionPlanPreferenceView(item));
        }
        return views;
    }

    public static double SumBatchedActionPlanPreferenceView(BatchedActionPlanPreferenceView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths;
    }
}
