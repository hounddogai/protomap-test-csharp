namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted policy ack bundle is in its life cycle.</summary>
public enum WeightedPolicyAckBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a weighted policy ack bundle and its current status.</summary>
public sealed class WeightedPolicyAckBundle(string code, string label, double rating, string badge, int batchSize, WeightedPolicyAckBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public string Badge { get; set; } = badge;
    public int BatchSize { get; set; } = batchSize;
    public WeightedPolicyAckBundleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted policy ack bundle in status proposed.</summary>
    public static WeightedPolicyAckBundle NewWeightedPolicyAckBundle(string code, string label)
    {
        return new WeightedPolicyAckBundle(code, label, 0.0, "", 0, WeightedPolicyAckBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedPolicyAckBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedPolicyAckBundleStatus next)
    {
        switch (Status)
        {
            case WeightedPolicyAckBundleStatus.Proposed:
                return next == WeightedPolicyAckBundleStatus.Approved || next == WeightedPolicyAckBundleStatus.Rejected;
            case WeightedPolicyAckBundleStatus.Approved:
                return next == WeightedPolicyAckBundleStatus.Withdrawn;
            case WeightedPolicyAckBundleStatus.Rejected:
                return next == WeightedPolicyAckBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedPolicyAckBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted policy ack bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedExposure()
    {
        return Math.Round(Rating - (double)BatchSize, 2);
    }

    public WeightedPolicyAckBundle Copy()
    {
        return new WeightedPolicyAckBundle(Code, Label, Rating, Badge, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
