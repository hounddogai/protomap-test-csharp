namespace Acme.Workforce.Contractors.Rate.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict rate proposal.</summary>
public sealed record StrictRateProposalView(string Code, string Label, string SourceText, string Version, string ContactEmailText, string StatusLabel)
{
    public static StrictRateProposalView ToStrictRateProposalView(StrictRateProposal item)
    {
        return new StrictRateProposalView(
            item.Code,
            item.Label,
            Text.Truncate(item.Source, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictRateProposalView> ToStrictRateProposalViews(List<StrictRateProposal> items)
    {
        var views = new List<StrictRateProposalView>();
        foreach (var item in items)
        {
            views.Add(ToStrictRateProposalView(item));
        }
        return views;
    }

    public static double SumStrictRateProposalView(StrictRateProposalView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedVersion;
    }
}
