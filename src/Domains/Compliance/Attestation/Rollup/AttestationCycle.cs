namespace Acme.Workforce.Compliance.Attestation.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation cycle is in its life cycle.</summary>
public enum AttestationCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One attestation cycle, with what the planners need to act on it.</summary>
public sealed class AttestationCycle(string code, string name, double ratio, double margin, bool archived, AttestationCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public double Margin { get; set; } = margin;
    public bool Archived { get; set; } = archived;
    public AttestationCycleStatus Status { get; set; } = status;

    /// <summary>Starts a attestation cycle in status planned.</summary>
    public static AttestationCycle NewAttestationCycle(string code, string name)
    {
        return new AttestationCycle(code, name, 0.0, 0.0, false, AttestationCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationCycleStatus next)
    {
        switch (Status)
        {
            case AttestationCycleStatus.Planned:
                switch (next)
                {
                    case AttestationCycleStatus.Completed:
                    case AttestationCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case AttestationCycleStatus.Scheduled:
                return next == AttestationCycleStatus.Completed;
            case AttestationCycleStatus.InProgress:
                return next == AttestationCycleStatus.Completed || next == AttestationCycleStatus.Cancelled;
            case AttestationCycleStatus.Completed:
                switch (next)
                {
                    case AttestationCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"attestation cycle"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Ratio * Margin, 2);
    }

    public AttestationCycle Copy()
    {
        return new AttestationCycle(Code, Name, Ratio, Margin, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
