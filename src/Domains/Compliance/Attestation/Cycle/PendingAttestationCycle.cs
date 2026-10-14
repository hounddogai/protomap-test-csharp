namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending attestation cycle is in its life cycle.</summary>
public enum PendingAttestationCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pending attestation cycle, with what the planners need to act on it.</summary>
public sealed class PendingAttestationCycle(string code, string name, int tenureMonths, double maximum, string description, string grade, PendingAttestationCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Maximum { get; set; } = maximum;
    public string Description { get; set; } = description;
    public string Grade { get; set; } = grade;
    public PendingAttestationCycleStatus Status { get; set; } = status;

    /// <summary>Starts a pending attestation cycle in status planned.</summary>
    public static PendingAttestationCycle NewPendingAttestationCycle(string code, string name)
    {
        return new PendingAttestationCycle(code, name, 0, 0.0, "", "", PendingAttestationCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAttestationCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAttestationCycleStatus next)
    {
        switch (Status)
        {
            case PendingAttestationCycleStatus.Planned:
                switch (next)
                {
                    case PendingAttestationCycleStatus.Scheduled:
                    case PendingAttestationCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingAttestationCycleStatus.Scheduled:
                return next == PendingAttestationCycleStatus.InProgress || next == PendingAttestationCycleStatus.Completed;
            case PendingAttestationCycleStatus.InProgress:
                return next == PendingAttestationCycleStatus.Completed || next == PendingAttestationCycleStatus.Cancelled;
            case PendingAttestationCycleStatus.Completed:
                return next == PendingAttestationCycleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingAttestationCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending attestation cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round((double)TenureMonths * Maximum, 2);
    }

    public PendingAttestationCycle Copy()
    {
        return new PendingAttestationCycle(Code, Name, TenureMonths, Maximum, Description, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
