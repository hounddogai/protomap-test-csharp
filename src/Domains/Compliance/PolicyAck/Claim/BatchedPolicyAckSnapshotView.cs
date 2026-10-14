namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched policy ack snapshot.</summary>
public sealed record BatchedPolicyAckSnapshotView(string Code, string Title, string Ceiling, string ExternalRefText, string Margin, string StatusLabel)
{
    public static BatchedPolicyAckSnapshotView ToBatchedPolicyAckSnapshotView(BatchedPolicyAckSnapshot item)
    {
        return new BatchedPolicyAckSnapshotView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPolicyAckSnapshotView> ToBatchedPolicyAckSnapshotViews(List<BatchedPolicyAckSnapshot> items)
    {
        var views = new List<BatchedPolicyAckSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPolicyAckSnapshotView(item));
        }
        return views;
    }

    public static double SumBatchedPolicyAckSnapshotView(BatchedPolicyAckSnapshotView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedMargin;
    }
}
