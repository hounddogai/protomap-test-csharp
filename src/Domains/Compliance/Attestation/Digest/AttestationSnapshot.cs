namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation snapshot is in its life cycle.</summary>
public enum AttestationSnapshotStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a attestation snapshot and its current status.</summary>
public sealed class AttestationSnapshot(string code, string label, int version, int createdAt, double budget, bool approved, AttestationSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public bool Approved { get; set; } = approved;
    public AttestationSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a attestation snapshot in status idle.</summary>
    public static AttestationSnapshot NewAttestationSnapshot(string code, string label)
    {
        return new AttestationSnapshot(code, label, 0, 0, 0.0, false, AttestationSnapshotStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationSnapshotStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationSnapshotStatus next)
    {
        switch (Status)
        {
            case AttestationSnapshotStatus.Idle:
                return next == AttestationSnapshotStatus.Stopped;
            case AttestationSnapshotStatus.Warming:
                switch (next)
                {
                    case AttestationSnapshotStatus.Ready:
                        return true;
                }
                return false;
            case AttestationSnapshotStatus.Ready:
                switch (next)
                {
                    case AttestationSnapshotStatus.Draining:
                        return true;
                }
                return false;
            case AttestationSnapshotStatus.Draining:
                return next == AttestationSnapshotStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AttestationSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Version + (double)CreatedAt, 2);
    }

    public AttestationSnapshot Copy()
    {
        return new AttestationSnapshot(Code, Label, Version, CreatedAt, Budget, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
