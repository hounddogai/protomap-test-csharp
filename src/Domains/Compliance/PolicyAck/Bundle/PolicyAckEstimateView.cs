namespace Acme.Workforce.Compliance.PolicyAck.Bundle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack estimate.</summary>
public sealed record PolicyAckEstimateView(string Code, string Name, string Priority, string Budget, string PinnedText, string Region, string StatusLabel)
{
    public static PolicyAckEstimateView ToPolicyAckEstimateView(PolicyAckEstimate item)
    {
        return new PolicyAckEstimateView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Region, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckEstimateView> ToPolicyAckEstimateViews(List<PolicyAckEstimate> items)
    {
        var views = new List<PolicyAckEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckEstimateView(item));
        }
        return views;
    }

    public static double SumPolicyAckEstimateView(PolicyAckEstimateView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedPriority + parsedBudget;
    }
}
