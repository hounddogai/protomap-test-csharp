namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted adjustment draft.</summary>
public sealed record WeightedAdjustmentDraftView(string Code, string Label, string Ceiling, string Approved, string NotesText, string Weight, string BatchSize, string Fte, string LeaveDays, string SummaryText, string StatusLabel)
{
    public static WeightedAdjustmentDraftView ToWeightedAdjustmentDraftView(WeightedAdjustmentDraft item)
    {
        return new WeightedAdjustmentDraftView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.Notes,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedAdjustmentDraftView> ToWeightedAdjustmentDraftViews(List<WeightedAdjustmentDraft> items)
    {
        var views = new List<WeightedAdjustmentDraftView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedAdjustmentDraftView(item));
        }
        return views;
    }

    public static double SumWeightedAdjustmentDraftView(WeightedAdjustmentDraftView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedWeight + (double)parsedBatchSize;
    }
}
