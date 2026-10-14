namespace Acme.Workforce.Compliance.PolicyAck.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack segment is in its life cycle.</summary>
public enum PolicyAckSegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A policy ack segment. Its code is unique within a tenant.</summary>
public sealed class PolicyAckSegment(string code, string label, double minimum, double cost, int batchSize, bool pinned, int leaveDays, string reviewer, string costCenter, string source, PolicyAckSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Cost { get; set; } = cost;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public int LeaveDays { get; set; } = leaveDays;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public string Source { get; set; } = source;
    public PolicyAckSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack segment in status open.</summary>
    public static PolicyAckSegment NewPolicyAckSegment(string code, string label)
    {
        return new PolicyAckSegment(code, label, 0.0, 0.0, 0, false, 0, "", "", "", PolicyAckSegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckSegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckSegmentStatus next)
    {
        switch (Status)
        {
            case PolicyAckSegmentStatus.Open:
                switch (next)
                {
                    case PolicyAckSegmentStatus.InReview:
                    case PolicyAckSegmentStatus.Resolved:
                        return true;
                }
                return false;
            case PolicyAckSegmentStatus.InReview:
                switch (next)
                {
                    case PolicyAckSegmentStatus.Reopened:
                    case PolicyAckSegmentStatus.Archived:
                        return true;
                }
                return false;
            case PolicyAckSegmentStatus.Resolved:
                return next == PolicyAckSegmentStatus.Reopened || next == PolicyAckSegmentStatus.Archived;
            case PolicyAckSegmentStatus.Reopened:
                switch (next)
                {
                    case PolicyAckSegmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Cost * Minimum, 2);
    }

    public PolicyAckSegment Copy()
    {
        return new PolicyAckSegment(
            Code,
            Label,
            Minimum,
            Cost,
            BatchSize,
            Pinned,
            LeaveDays,
            Reviewer,
            CostCenter,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
