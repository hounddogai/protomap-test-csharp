namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched policy ack snapshot is in its life cycle.</summary>
public enum BatchedPolicyAckSnapshotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched policy ack snapshot. Its code is unique within a tenant.</summary>
public sealed class BatchedPolicyAckSnapshot(string code, string title, double ceiling, string externalRef, double margin, BatchedPolicyAckSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public string ExternalRef { get; set; } = externalRef;
    public double Margin { get; set; } = margin;
    public BatchedPolicyAckSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a batched policy ack snapshot in status requested.</summary>
    public static BatchedPolicyAckSnapshot NewBatchedPolicyAckSnapshot(string code, string title)
    {
        return new BatchedPolicyAckSnapshot(code, title, 0.0, "", 0.0, BatchedPolicyAckSnapshotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPolicyAckSnapshotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPolicyAckSnapshotStatus next)
    {
        switch (Status)
        {
            case BatchedPolicyAckSnapshotStatus.Requested:
                switch (next)
                {
                    case BatchedPolicyAckSnapshotStatus.Granted:
                    case BatchedPolicyAckSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case BatchedPolicyAckSnapshotStatus.Granted:
                switch (next)
                {
                    case BatchedPolicyAckSnapshotStatus.Revoked:
                    case BatchedPolicyAckSnapshotStatus.Expired:
                        return true;
                }
                return false;
            case BatchedPolicyAckSnapshotStatus.Revoked:
                switch (next)
                {
                    case BatchedPolicyAckSnapshotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedPolicyAckSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched policy ack snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Margin + Ceiling, 2);
    }

    public BatchedPolicyAckSnapshot Copy()
    {
        return new BatchedPolicyAckSnapshot(Code, Title, Ceiling, ExternalRef, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
