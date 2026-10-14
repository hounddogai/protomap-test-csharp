namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using System.Globalization;

/// <summary>How screens and exports show a pending policy ack group.</summary>
public sealed record PendingPolicyAckGroupView(string Code, string Title, string Threshold, string Budget, string UpdatedAt, string Amount, string StatusLabel)
{
    public static PendingPolicyAckGroupView ToPendingPolicyAckGroupView(PendingPolicyAckGroup item)
    {
        return new PendingPolicyAckGroupView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingPolicyAckGroupView> ToPendingPolicyAckGroupViews(List<PendingPolicyAckGroup> items)
    {
        var views = new List<PendingPolicyAckGroupView>();
        foreach (var item in items)
        {
            views.Add(ToPendingPolicyAckGroupView(item));
        }
        return views;
    }

    public static double SumPendingPolicyAckGroupView(PendingPolicyAckGroupView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedBudget + (double)parsedUpdatedAt;
    }
}
