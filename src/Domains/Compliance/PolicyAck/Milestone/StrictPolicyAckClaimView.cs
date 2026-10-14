namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict policy ack claim.</summary>
public sealed record StrictPolicyAckClaimView(string Code, string Label, string Channel, string SkillText, string Minimum, string Discount, string Attempts, string Summary, string StatusLabel)
{
    public static StrictPolicyAckClaimView ToStrictPolicyAckClaimView(StrictPolicyAckClaim item)
    {
        return new StrictPolicyAckClaimView(
            item.Code,
            item.Label,
            item.Channel,
            Text.Truncate(item.Skill, 80),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictPolicyAckClaimView> ToStrictPolicyAckClaimViews(List<StrictPolicyAckClaim> items)
    {
        var views = new List<StrictPolicyAckClaimView>();
        foreach (var item in items)
        {
            views.Add(ToStrictPolicyAckClaimView(item));
        }
        return views;
    }

    public static double SumStrictPolicyAckClaimView(StrictPolicyAckClaimView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedDiscount + (double)parsedAttempts;
    }
}
