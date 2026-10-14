namespace Acme.Workforce.Orgdesign.Span.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched span allocation.</summary>
public sealed record BatchedSpanAllocationView(string Code, string LabelText, string CreatedAtText, string UnitText, string Grade, string ContactEmailText, string StatusLabel)
{
    public static BatchedSpanAllocationView ToBatchedSpanAllocationView(BatchedSpanAllocation item)
    {
        return new BatchedSpanAllocationView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Unit, 40),
            item.Grade,
            Text.Truncate(item.ContactEmail, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedSpanAllocationView> ToBatchedSpanAllocationViews(List<BatchedSpanAllocation> items)
    {
        var views = new List<BatchedSpanAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedSpanAllocationView(item));
        }
        return views;
    }

    public static double SumBatchedSpanAllocationView(BatchedSpanAllocationView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt;
    }
}
