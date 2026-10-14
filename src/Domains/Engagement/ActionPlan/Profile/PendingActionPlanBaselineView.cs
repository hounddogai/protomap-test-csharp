namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pending action plan baseline.</summary>
public sealed record PendingActionPlanBaselineView(string Code, string Name, string WindowDays, string RevisionText, string Attempts, string EndsAt, string Label, string EmployeeId, string MinimumText, string StatusLabel)
{
    public static PendingActionPlanBaselineView ToPendingActionPlanBaselineView(PendingActionPlanBaseline item)
    {
        return new PendingActionPlanBaselineView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Label,
            item.EmployeeId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingActionPlanBaselineView> ToPendingActionPlanBaselineViews(List<PendingActionPlanBaseline> items)
    {
        var views = new List<PendingActionPlanBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToPendingActionPlanBaselineView(item));
        }
        return views;
    }

    public static double SumPendingActionPlanBaselineView(PendingActionPlanBaselineView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedRevision + (double)parsedAttempts;
    }
}
