namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive policy ack proposal.</summary>
public sealed record AdaptivePolicyAckProposalView(string Code, string NameText, string SalaryBand, string Badge, string Description, string ExpiresAtText, string VersionText, string Weight, string RetriesText, string Limit, string MarginText, string StatusLabel)
{
    public static AdaptivePolicyAckProposalView ToAdaptivePolicyAckProposalView(AdaptivePolicyAckProposal item)
    {
        return new AdaptivePolicyAckProposalView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.SalaryBand,
            item.Badge,
            item.Description,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePolicyAckProposalView> ToAdaptivePolicyAckProposalViews(List<AdaptivePolicyAckProposal> items)
    {
        var views = new List<AdaptivePolicyAckProposalView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePolicyAckProposalView(item));
        }
        return views;
    }

    public static double SumAdaptivePolicyAckProposalView(AdaptivePolicyAckProposalView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedVersion + parsedWeight;
    }
}
