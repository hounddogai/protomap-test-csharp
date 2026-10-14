namespace Acme.Workforce.Compliance.Attestation.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation mapping is in its life cycle.</summary>
public enum AttestationMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A attestation mapping. Its code is unique within a tenant.</summary>
public sealed class AttestationMapping(string code, string label, string externalRef, double hours, double score, double weight, string channel, bool active, AttestationMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Hours { get; set; } = hours;
    public double Score { get; set; } = score;
    public double Weight { get; set; } = weight;
    public string Channel { get; set; } = channel;
    public bool Active { get; set; } = active;
    public AttestationMappingStatus Status { get; set; } = status;

    /// <summary>Starts a attestation mapping in status proposed.</summary>
    public static AttestationMapping NewAttestationMapping(string code, string label)
    {
        return new AttestationMapping(code, label, "", 0.0, 0.0, 0.0, "", false, AttestationMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationMappingStatus next)
    {
        switch (Status)
        {
            case AttestationMappingStatus.Proposed:
                switch (next)
                {
                    case AttestationMappingStatus.Withdrawn:
                        return true;
                }
                return false;
            case AttestationMappingStatus.Approved:
                return next == AttestationMappingStatus.Withdrawn;
            case AttestationMappingStatus.Rejected:
                switch (next)
                {
                    case AttestationMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Weight * Hours, 2);
    }

    public AttestationMapping Copy()
    {
        return new AttestationMapping(Code, Label, ExternalRef, Hours, Score, Weight, Channel, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
