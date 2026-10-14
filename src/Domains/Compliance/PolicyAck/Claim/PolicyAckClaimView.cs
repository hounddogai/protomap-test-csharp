namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack claim.</summary>
public sealed record PolicyAckClaimView(string Code, string Title, string Category, string Amount, string Location, string NameText, string ExternalRefText, string TenureMonths, string SegmentText, string Phone, string StatusLabel)
{
    public static PolicyAckClaimView ToPolicyAckClaimView(PolicyAckClaim item)
    {
        return new PolicyAckClaimView(
            item.Code,
            Text.Truncate(item.Title, 40),
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            Text.Truncate(item.Name, 80),
            item.ExternalRef,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Segment, 120),
            item.Phone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckClaimView> ToPolicyAckClaimViews(List<PolicyAckClaim> items)
    {
        var views = new List<PolicyAckClaimView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckClaimView(item));
        }
        return views;
    }

    public static double SumPolicyAckClaimView(PolicyAckClaimView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedTenureMonths;
    }
}
