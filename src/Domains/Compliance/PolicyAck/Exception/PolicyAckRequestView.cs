namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack request.</summary>
public sealed record PolicyAckRequestView(string CodeText, string TitleText, string TenureMonths, string ExpiresAtText, string RevisionText, string BatchSizeText, string SegmentText, string Ratio, string Reviewer, string Sequence, string StatusLabel)
{
    public static PolicyAckRequestView ToPolicyAckRequestView(PolicyAckRequest item)
    {
        return new PolicyAckRequestView(
            item.Code,
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 40),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckRequestView> ToPolicyAckRequestViews(List<PolicyAckRequest> items)
    {
        var views = new List<PolicyAckRequestView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckRequestView(item));
        }
        return views;
    }

    public static double SumPolicyAckRequestView(PolicyAckRequestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.RevisionText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedExpiresAt + (double)parsedRevision;
    }
}
