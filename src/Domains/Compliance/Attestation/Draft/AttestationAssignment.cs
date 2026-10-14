namespace Acme.Workforce.Compliance.Attestation.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation assignment is in its life cycle.</summary>
public enum AttestationAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A attestation assignment as the attestation team keeps it.</summary>
public sealed class AttestationAssignment(string code, string label, double weight, int createdAt, int sequence, string contactEmail, string notes, int attempts, AttestationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int CreatedAt { get; set; } = createdAt;
    public int Sequence { get; set; } = sequence;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public int Attempts { get; set; } = attempts;
    public AttestationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a attestation assignment in status proposed.</summary>
    public static AttestationAssignment NewAttestationAssignment(string code, string label)
    {
        return new AttestationAssignment(code, label, 0.0, 0, 0, "", "", 0, AttestationAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationAssignmentStatus next)
    {
        switch (Status)
        {
            case AttestationAssignmentStatus.Proposed:
                return next == AttestationAssignmentStatus.Approved || next == AttestationAssignmentStatus.Rejected;
            case AttestationAssignmentStatus.Approved:
                switch (next)
                {
                    case AttestationAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case AttestationAssignmentStatus.Rejected:
                return next == AttestationAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AttestationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts - (double)Sequence, 2);
    }

    public AttestationAssignment Copy()
    {
        return new AttestationAssignment(
            Code,
            Label,
            Weight,
            CreatedAt,
            Sequence,
            ContactEmail,
            Notes,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
