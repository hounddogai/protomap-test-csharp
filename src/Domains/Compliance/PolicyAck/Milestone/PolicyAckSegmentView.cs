namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a policy ack segment.</summary>
public sealed record PolicyAckSegmentView(string Code, string LabelText, string Minimum, string Cost, string BatchSize, string Pinned, string LeaveDays, string Reviewer, string CostCenter, string Source, string StatusLabel)
{
    public static PolicyAckSegmentView ToPolicyAckSegmentView(PolicyAckSegment item)
    {
        return new PolicyAckSegmentView(
            item.Code,
            item.Label,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 120),
            item.CostCenter,
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyAckSegmentView> ToPolicyAckSegmentViews(List<PolicyAckSegment> items)
    {
        var views = new List<PolicyAckSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyAckSegmentView(item));
        }
        return views;
    }

    public static double SumPolicyAckSegmentView(PolicyAckSegmentView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedCost + (double)parsedBatchSize;
    }
}
