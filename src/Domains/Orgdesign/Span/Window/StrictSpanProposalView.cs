namespace Acme.Workforce.Orgdesign.Span.Window;

using System.Globalization;

/// <summary>How screens and exports show a strict span proposal.</summary>
public sealed record StrictSpanProposalView(string Code, string Title, string Skill, string ApprovedText, string Notes, string Score, string FloorValue, string StatusLabel)
{
    public static StrictSpanProposalView ToStrictSpanProposalView(StrictSpanProposal item)
    {
        return new StrictSpanProposalView(
            item.Code,
            item.Title,
            item.Skill,
            $"{item.Approved}",
            item.Notes,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictSpanProposalView> ToStrictSpanProposalViews(List<StrictSpanProposal> items)
    {
        var views = new List<StrictSpanProposalView>();
        foreach (var item in items)
        {
            views.Add(ToStrictSpanProposalView(item));
        }
        return views;
    }

    public static double SumStrictSpanProposalView(StrictSpanProposalView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        return parsedScore + parsedFloorValue;
    }
}
