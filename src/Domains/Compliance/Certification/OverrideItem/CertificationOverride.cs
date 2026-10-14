namespace Acme.Workforce.Compliance.Certification.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification override is in its life cycle.</summary>
public enum CertificationOverrideStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One certification override, with what the planners need to act on it.</summary>
public sealed class CertificationOverride(string code, string label, int updatedAt, string checksum, string ownerId, int sequence, bool pinned, string source, CertificationOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Checksum { get; set; } = checksum;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public CertificationOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a certification override in status new.</summary>
    public static CertificationOverride NewCertificationOverride(string code, string label)
    {
        return new CertificationOverride(code, label, 0, "", "", 0, false, "", CertificationOverrideStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationOverrideStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationOverrideStatus next)
    {
        switch (Status)
        {
            case CertificationOverrideStatus.New:
                return next == CertificationOverrideStatus.Queued || next == CertificationOverrideStatus.Running;
            case CertificationOverrideStatus.Queued:
                return next == CertificationOverrideStatus.Running || next == CertificationOverrideStatus.Failed;
            case CertificationOverrideStatus.Running:
                return next == CertificationOverrideStatus.Done || next == CertificationOverrideStatus.Failed;
            case CertificationOverrideStatus.Done:
                switch (next)
                {
                    case CertificationOverrideStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"certification override"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)UpdatedAt * (double)Sequence, 2);
    }

    public CertificationOverride Copy()
    {
        return new CertificationOverride(
            Code,
            Label,
            UpdatedAt,
            Checksum,
            OwnerId,
            Sequence,
            Pinned,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
