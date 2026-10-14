namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive offboarding ledger is in its life cycle.</summary>
public enum AdaptiveOffboardingLedgerStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adaptive offboarding ledger, with what the planners need to act on it.</summary>
public sealed class AdaptiveOffboardingLedger(string code, string name, double maximum, string region, string skill, int durationMinutes, AdaptiveOffboardingLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Region { get; set; } = region;
    public string Skill { get; set; } = skill;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AdaptiveOffboardingLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive offboarding ledger in status planned.</summary>
    public static AdaptiveOffboardingLedger NewAdaptiveOffboardingLedger(string code, string name)
    {
        return new AdaptiveOffboardingLedger(code, name, 0.0, "", "", 0, AdaptiveOffboardingLedgerStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveOffboardingLedgerStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveOffboardingLedgerStatus next)
    {
        switch (Status)
        {
            case AdaptiveOffboardingLedgerStatus.Planned:
                switch (next)
                {
                    case AdaptiveOffboardingLedgerStatus.InProgress:
                        return true;
                }
                return false;
            case AdaptiveOffboardingLedgerStatus.Scheduled:
                return next == AdaptiveOffboardingLedgerStatus.InProgress || next == AdaptiveOffboardingLedgerStatus.Cancelled;
            case AdaptiveOffboardingLedgerStatus.InProgress:
                return next == AdaptiveOffboardingLedgerStatus.Completed || next == AdaptiveOffboardingLedgerStatus.Cancelled;
            case AdaptiveOffboardingLedgerStatus.Completed:
                switch (next)
                {
                    case AdaptiveOffboardingLedgerStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveOffboardingLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive offboarding ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Maximum + (double)DurationMinutes, 2);
    }

    public AdaptiveOffboardingLedger Copy()
    {
        return new AdaptiveOffboardingLedger(Code, Name, Maximum, Region, Skill, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
