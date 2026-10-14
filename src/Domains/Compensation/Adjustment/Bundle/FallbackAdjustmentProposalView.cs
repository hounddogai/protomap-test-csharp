namespace Acme.Workforce.Compensation.Adjustment.Bundle;

using System.Globalization;

/// <summary>How screens and exports show a fallback adjustment proposal.</summary>
public sealed record FallbackAdjustmentProposalView(string Code, string Label, string Sequence, string Fte, string BatchSize, string WindowDaysText, string PinnedText, string Discount, string ExternalRef, string SegmentText, string StatusLabel)
{
    public static FallbackAdjustmentProposalView ToFallbackAdjustmentProposalView(FallbackAdjustmentProposal item)
    {
        return new FallbackAdjustmentProposalView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Segment,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackAdjustmentProposalView> ToFallbackAdjustmentProposalViews(List<FallbackAdjustmentProposal> items)
    {
        var views = new List<FallbackAdjustmentProposalView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackAdjustmentProposalView(item));
        }
        return views;
    }

    public static double SumFallbackAdjustmentProposalView(FallbackAdjustmentProposalView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedSequence + parsedFte + (double)parsedBatchSize;
    }
}
