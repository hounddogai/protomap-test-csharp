namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling nomination mapping is in its life cycle.</summary>
public enum RollingNominationMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One rolling nomination mapping, with what the planners need to act on it.</summary>
public sealed class RollingNominationMapping(string code, string label, string employeeId, string ownerId, int sequence, bool pinned, string externalRef, bool active, string contactEmail, string title, RollingNominationMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string OwnerId { get; set; } = ownerId;
    public int Sequence { get; set; } = sequence;
    public bool Pinned { get; set; } = pinned;
    public string ExternalRef { get; set; } = externalRef;
    public bool Active { get; set; } = active;
    public string ContactEmail { get; set; } = contactEmail;
    public string Title { get; set; } = title;
    public RollingNominationMappingStatus Status { get; set; } = status;

    /// <summary>Starts a rolling nomination mapping in status proposed.</summary>
    public static RollingNominationMapping NewRollingNominationMapping(string code, string label)
    {
        return new RollingNominationMapping(
            code,
            label,
            "",
            "",
            0,
            false,
            "",
            false,
            "",
            "",
            RollingNominationMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingNominationMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingNominationMappingStatus next)
    {
        switch (Status)
        {
            case RollingNominationMappingStatus.Proposed:
                return next == RollingNominationMappingStatus.Approved;
            case RollingNominationMappingStatus.Approved:
                return next == RollingNominationMappingStatus.Rejected || next == RollingNominationMappingStatus.Withdrawn;
            case RollingNominationMappingStatus.Rejected:
                return next == RollingNominationMappingStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RollingNominationMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling nomination mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling nomination mapping"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public RollingNominationMapping Copy()
    {
        return new RollingNominationMapping(
            Code,
            Label,
            EmployeeId,
            OwnerId,
            Sequence,
            Pinned,
            ExternalRef,
            Active,
            ContactEmail,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
