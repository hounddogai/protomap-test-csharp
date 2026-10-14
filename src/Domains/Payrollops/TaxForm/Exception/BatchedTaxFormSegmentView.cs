namespace Acme.Workforce.Payrollops.TaxForm.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched tax form segment.</summary>
public sealed record BatchedTaxFormSegmentView(string CodeText, string LabelText, string CostCenter, string Skill, string PinnedText, string StartsAt, string StatusLabel)
{
    public static BatchedTaxFormSegmentView ToBatchedTaxFormSegmentView(BatchedTaxFormSegment item)
    {
        return new BatchedTaxFormSegmentView(
            item.Code,
            Text.Truncate(item.Label, 40),
            item.CostCenter,
            Text.Truncate(item.Skill, 64),
            $"{item.Pinned}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedTaxFormSegmentView> ToBatchedTaxFormSegmentViews(List<BatchedTaxFormSegment> items)
    {
        var views = new List<BatchedTaxFormSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedTaxFormSegmentView(item));
        }
        return views;
    }

    public static double SumBatchedTaxFormSegmentView(BatchedTaxFormSegmentView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
