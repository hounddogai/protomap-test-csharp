namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback policy ack snapshot.</summary>
public sealed record FallbackPolicyAckSnapshotView(string Code, string Name, string EndsAtText, string Threshold, string PriorityText, string Checksum, string DurationMinutes, string StatusLabel)
{
    public static FallbackPolicyAckSnapshotView ToFallbackPolicyAckSnapshotView(FallbackPolicyAckSnapshot item)
    {
        return new FallbackPolicyAckSnapshotView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPolicyAckSnapshotView> ToFallbackPolicyAckSnapshotViews(List<FallbackPolicyAckSnapshot> items)
    {
        var views = new List<FallbackPolicyAckSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPolicyAckSnapshotView(item));
        }
        return views;
    }

    public static double SumFallbackPolicyAckSnapshotView(FallbackPolicyAckSnapshotView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAtText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedThreshold + (double)parsedPriority;
    }
}
