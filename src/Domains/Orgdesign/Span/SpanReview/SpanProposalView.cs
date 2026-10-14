namespace Acme.Workforce.Orgdesign.Span.SpanReview;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a span proposal.</summary>
public sealed record SpanProposalView(string Code, string Title, string SalaryBand, string ChecksumText, string Phone, string CreatedAt, string NameText, string SegmentText, string Score, string Budget, string Amount, string StatusLabel)
{
    public static SpanProposalView ToSpanProposalView(SpanProposal item)
    {
        return new SpanProposalView(
            item.Code,
            item.Title,
            Text.Truncate(item.SalaryBand, 40),
            item.Checksum,
            Text.Truncate(item.Phone, 64),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Segment,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SpanProposalView> ToSpanProposalViews(List<SpanProposal> items)
    {
        var views = new List<SpanProposalView>();
        foreach (var item in items)
        {
            views.Add(ToSpanProposalView(item));
        }
        return views;
    }

    public static double SumSpanProposalView(SpanProposalView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + parsedScore + parsedBudget;
    }
}
