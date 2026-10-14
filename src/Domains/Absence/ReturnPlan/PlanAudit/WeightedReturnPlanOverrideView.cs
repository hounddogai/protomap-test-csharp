namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted return plan override.</summary>
public sealed record WeightedReturnPlanOverrideView(string CodeText, string Label, string Score, string JobCode, string WindowDays, string RevisionText, string Category, string OwnerIdText, string Timezone, string StatusLabel)
{
    public static WeightedReturnPlanOverrideView ToWeightedReturnPlanOverrideView(WeightedReturnPlanOverride item)
    {
        return new WeightedReturnPlanOverrideView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.JobCode,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 120),
            item.OwnerId,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedReturnPlanOverrideView> ToWeightedReturnPlanOverrideViews(List<WeightedReturnPlanOverride> items)
    {
        var views = new List<WeightedReturnPlanOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedReturnPlanOverrideView(item));
        }
        return views;
    }

    public static double SumWeightedReturnPlanOverrideView(WeightedReturnPlanOverrideView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedWindowDays + (double)parsedRevision;
    }
}
