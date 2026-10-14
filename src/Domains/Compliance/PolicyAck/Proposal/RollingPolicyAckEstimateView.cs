namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using System.Globalization;

/// <summary>How screens and exports show a rolling policy ack estimate.</summary>
public sealed record RollingPolicyAckEstimateView(string Code, string Title, string BatchSizeText, string Budget, string Rate, string Visible, string DurationMinutes, string StatusLabel)
{
    public static RollingPolicyAckEstimateView ToRollingPolicyAckEstimateView(RollingPolicyAckEstimate item)
    {
        return new RollingPolicyAckEstimateView(
            item.Code,
            item.Title,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPolicyAckEstimateView> ToRollingPolicyAckEstimateViews(List<RollingPolicyAckEstimate> items)
    {
        var views = new List<RollingPolicyAckEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPolicyAckEstimateView(item));
        }
        return views;
    }

    public static double SumRollingPolicyAckEstimateView(RollingPolicyAckEstimateView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedBudget + parsedRate;
    }
}
