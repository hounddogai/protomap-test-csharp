namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy ack snapshot is in its life cycle.</summary>
public enum FallbackPolicyAckSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback policy ack snapshot. Its code is unique within a tenant.</summary>
public sealed class FallbackPolicyAckSnapshot(string code, string name, int endsAt, double threshold, int priority, string checksum, int durationMinutes, FallbackPolicyAckSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public double Threshold { get; set; } = threshold;
    public int Priority { get; set; } = priority;
    public string Checksum { get; set; } = checksum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public FallbackPolicyAckSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy ack snapshot in status requested.</summary>
    public static FallbackPolicyAckSnapshot NewFallbackPolicyAckSnapshot(string code, string name)
    {
        return new FallbackPolicyAckSnapshot(
            code,
            name,
            0,
            0.0,
            0,
            "",
            0,
            FallbackPolicyAckSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyAckSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyAckSnapshotStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyAckSnapshotStatus.Requested:
                switch (next)
                {
                    case FallbackPolicyAckSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case FallbackPolicyAckSnapshotStatus.Granted:
                return next == FallbackPolicyAckSnapshotStatus.Revoked;
            case FallbackPolicyAckSnapshotStatus.Revoked:
                switch (next)
                {
                    case FallbackPolicyAckSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyAckSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy ack snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback policy ack snapshot"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Priority * (double)DurationMinutes, 2);
    }

    public FallbackPolicyAckSnapshot Copy()
    {
        return new FallbackPolicyAckSnapshot(
            Code,
            Name,
            EndsAt,
            Threshold,
            Priority,
            Checksum,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
