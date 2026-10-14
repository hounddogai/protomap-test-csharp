namespace Acme.Workforce.Compliance.Attestation.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation milestone is in its life cycle.</summary>
public enum AttestationMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One attestation milestone, with what the planners need to act on it.</summary>
public sealed class AttestationMilestone(string code, string label, int durationMinutes, string name, int expiresAt, string notes, int revision, double ratio, int leaveDays, string description, string employeeId, AttestationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Notes { get; set; } = notes;
    public int Revision { get; set; } = revision;
    public double Ratio { get; set; } = ratio;
    public int LeaveDays { get; set; } = leaveDays;
    public string Description { get; set; } = description;
    public string EmployeeId { get; set; } = employeeId;
    public AttestationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a attestation milestone in status planned.</summary>
    public static AttestationMilestone NewAttestationMilestone(string code, string label)
    {
        return new AttestationMilestone(
            code,
            label,
            0,
            "",
            0,
            "",
            0,
            0.0,
            0,
            "",
            "",
            AttestationMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationMilestoneStatus next)
    {
        switch (Status)
        {
            case AttestationMilestoneStatus.Planned:
                return next == AttestationMilestoneStatus.Scheduled || next == AttestationMilestoneStatus.Completed;
            case AttestationMilestoneStatus.Scheduled:
                return next == AttestationMilestoneStatus.Completed || next == AttestationMilestoneStatus.Cancelled;
            case AttestationMilestoneStatus.InProgress:
                return next == AttestationMilestoneStatus.Completed || next == AttestationMilestoneStatus.Cancelled;
            case AttestationMilestoneStatus.Completed:
                switch (next)
                {
                    case AttestationMilestoneStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ratio - (double)DurationMinutes, 2);
    }

    public AttestationMilestone Copy()
    {
        return new AttestationMilestone(
            Code,
            Label,
            DurationMinutes,
            Name,
            ExpiresAt,
            Notes,
            Revision,
            Ratio,
            LeaveDays,
            Description,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
