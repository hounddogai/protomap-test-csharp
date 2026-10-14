namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a succession plan draft.</summary>
public sealed record SuccessionPlanDraftView(string CodeText, string Title, string Ratio, string Reviewer, string Phone, string RegionText, string JobCode, string Ceiling, string StatusLabel)
{
    public static SuccessionPlanDraftView ToSuccessionPlanDraftView(SuccessionPlanDraft item)
    {
        return new SuccessionPlanDraftView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            Text.Truncate(item.Phone, 64),
            item.Region,
            item.JobCode,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SuccessionPlanDraftView> ToSuccessionPlanDraftViews(List<SuccessionPlanDraft> items)
    {
        var views = new List<SuccessionPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToSuccessionPlanDraftView(item));
        }
        return views;
    }

    public static double SumSuccessionPlanDraftView(SuccessionPlanDraftView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        return parsedRatio + parsedCeiling;
    }
}
