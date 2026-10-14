namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation rollup is in its life cycle.</summary>
public enum AttestationRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One attestation rollup, with what the planners need to act on it.</summary>
public sealed class AttestationRollup(string code, string name, string contactEmail, bool active, string checksum, string employeeId, double maximum, AttestationRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Active { get; set; } = active;
    public string Checksum { get; set; } = checksum;
    public string EmployeeId { get; set; } = employeeId;
    public double Maximum { get; set; } = maximum;
    public AttestationRollupStatus Status { get; set; } = status;

    /// <summary>Starts a attestation rollup in status proposed.</summary>
    public static AttestationRollup NewAttestationRollup(string code, string name)
    {
        return new AttestationRollup(code, name, "", false, "", "", 0.0, AttestationRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationRollupStatus next)
    {
        switch (Status)
        {
            case AttestationRollupStatus.Proposed:
                switch (next)
                {
                    case AttestationRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case AttestationRollupStatus.Approved:
                switch (next)
                {
                    case AttestationRollupStatus.Rejected:
                        return true;
                }
                return false;
            case AttestationRollupStatus.Rejected:
                return next == AttestationRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AttestationRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public AttestationRollup Copy()
    {
        return new AttestationRollup(Code, Name, ContactEmail, Active, Checksum, EmployeeId, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
