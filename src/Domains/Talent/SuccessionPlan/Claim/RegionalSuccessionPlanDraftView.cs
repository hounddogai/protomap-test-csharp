namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional succession plan draft.</summary>
public sealed record RegionalSuccessionPlanDraftView(string Code, string Title, string Priority, string Reviewer, string ThresholdText, string Locale, string Segment, string AmountText, string StatusLabel)
{
    public static RegionalSuccessionPlanDraftView ToRegionalSuccessionPlanDraftView(RegionalSuccessionPlanDraft item)
    {
        return new RegionalSuccessionPlanDraftView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Locale, 120),
            Text.Truncate(item.Segment, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSuccessionPlanDraftView> ToRegionalSuccessionPlanDraftViews(List<RegionalSuccessionPlanDraft> items)
    {
        var views = new List<RegionalSuccessionPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSuccessionPlanDraftView(item));
        }
        return views;
    }

    public static double SumRegionalSuccessionPlanDraftView(RegionalSuccessionPlanDraftView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedThreshold + parsedAmount;
    }
}
