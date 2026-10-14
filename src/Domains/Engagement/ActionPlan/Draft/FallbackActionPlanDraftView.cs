namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback action plan draft.</summary>
public sealed record FallbackActionPlanDraftView(string Code, string Label, string GradeText, string Amount, string QuantityText, string BatchSize, string StatusLabel)
{
    public static FallbackActionPlanDraftView ToFallbackActionPlanDraftView(FallbackActionPlanDraft item)
    {
        return new FallbackActionPlanDraftView(
            Text.Truncate(item.Code, 64),
            item.Label,
            Text.Truncate(item.Grade, 120),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackActionPlanDraftView> ToFallbackActionPlanDraftViews(List<FallbackActionPlanDraft> items)
    {
        var views = new List<FallbackActionPlanDraftView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackActionPlanDraftView(item));
        }
        return views;
    }

    public static double SumFallbackActionPlanDraftView(FallbackActionPlanDraftView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedQuantity + (double)parsedBatchSize;
    }
}
