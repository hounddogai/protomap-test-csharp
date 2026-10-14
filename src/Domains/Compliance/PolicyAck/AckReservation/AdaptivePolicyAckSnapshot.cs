namespace Acme.Workforce.Compliance.PolicyAck.AckReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive policy ack snapshot is in its life cycle.</summary>
public enum AdaptivePolicyAckSnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One adaptive policy ack snapshot, with what the planners need to act on it.</summary>
public sealed class AdaptivePolicyAckSnapshot(string code, string label, double threshold, string badge, bool archived, AdaptivePolicyAckSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Badge { get; set; } = badge;
    public bool Archived { get; set; } = archived;
    public AdaptivePolicyAckSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive policy ack snapshot in status created.</summary>
    public static AdaptivePolicyAckSnapshot NewAdaptivePolicyAckSnapshot(string code, string label)
    {
        return new AdaptivePolicyAckSnapshot(code, label, 0.0, "", false, AdaptivePolicyAckSnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePolicyAckSnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePolicyAckSnapshotStatus next)
    {
        switch (Status)
        {
            case AdaptivePolicyAckSnapshotStatus.Created:
                return next == AdaptivePolicyAckSnapshotStatus.Published;
            case AdaptivePolicyAckSnapshotStatus.Validated:
                switch (next)
                {
                    case AdaptivePolicyAckSnapshotStatus.Published:
                    case AdaptivePolicyAckSnapshotStatus.Retired:
                        return true;
                }
                return false;
            case AdaptivePolicyAckSnapshotStatus.Published:
                return next == AdaptivePolicyAckSnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptivePolicyAckSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive policy ack snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive policy ack snapshot"} {Code}";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public AdaptivePolicyAckSnapshot Copy()
    {
        return new AdaptivePolicyAckSnapshot(Code, Label, Threshold, Badge, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
