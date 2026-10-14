namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive policy ack claim.</summary>
public sealed record AdaptivePolicyAckClaimView(string Code, string LabelText, string LeaveDaysText, string GraceDays, string Limit, string Source, string ManagerId, string TenureMonths, string Location, string StatusLabel)
{
    public static AdaptivePolicyAckClaimView ToAdaptivePolicyAckClaimView(AdaptivePolicyAckClaim item)
    {
        return new AdaptivePolicyAckClaimView(
            Text.Truncate(item.Code, 40),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Source,
            Text.Truncate(item.ManagerId, 40),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePolicyAckClaimView> ToAdaptivePolicyAckClaimViews(List<AdaptivePolicyAckClaim> items)
    {
        var views = new List<AdaptivePolicyAckClaimView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePolicyAckClaimView(item));
        }
        return views;
    }

    public static double SumAdaptivePolicyAckClaimView(AdaptivePolicyAckClaimView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedGraceDays + (double)parsedLimit;
    }
}
