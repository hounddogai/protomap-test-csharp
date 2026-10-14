namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict floor plan draft.</summary>
public sealed record StrictFloorPlanDraftView(string CodeText, string Label, string AmountText, string Retries, string BatchSizeText, string MarginText, string Attempts, string Discount, string CreatedAt, string Rating, string Team, string StatusLabel)
{
    public static StrictFloorPlanDraftView ToStrictFloorPlanDraftView(StrictFloorPlanDraft item)
    {
        return new StrictFloorPlanDraftView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Team, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictFloorPlanDraftView> ToStrictFloorPlanDraftViews(List<StrictFloorPlanDraft> items)
    {
        var views = new List<StrictFloorPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToStrictFloorPlanDraftView(item));
        }
        return views;
    }

    public static double SumStrictFloorPlanDraftView(StrictFloorPlanDraftView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedRetries + (double)parsedBatchSize;
    }
}
