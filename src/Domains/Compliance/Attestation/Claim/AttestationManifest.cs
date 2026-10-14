namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation manifest is in its life cycle.</summary>
public enum AttestationManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A attestation manifest as the attestation team keeps it.</summary>
public sealed class AttestationManifest(string code, string label, string title, double hours, double amount, int version, AttestationManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public double Amount { get; set; } = amount;
    public int Version { get; set; } = version;
    public AttestationManifestStatus Status { get; set; } = status;

    /// <summary>Starts a attestation manifest in status planned.</summary>
    public static AttestationManifest NewAttestationManifest(string code, string label)
    {
        return new AttestationManifest(code, label, "", 0.0, 0.0, 0, AttestationManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationManifestStatus next)
    {
        switch (Status)
        {
            case AttestationManifestStatus.Planned:
                switch (next)
                {
                    case AttestationManifestStatus.Scheduled:
                    case AttestationManifestStatus.InProgress:
                        return true;
                }
                return false;
            case AttestationManifestStatus.Scheduled:
                switch (next)
                {
                    case AttestationManifestStatus.Completed:
                        return true;
                }
                return false;
            case AttestationManifestStatus.InProgress:
                return next == AttestationManifestStatus.Completed;
            case AttestationManifestStatus.Completed:
                switch (next)
                {
                    case AttestationManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Hours + Amount, 2);
    }

    public AttestationManifest Copy()
    {
        return new AttestationManifest(Code, Label, Title, Hours, Amount, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
