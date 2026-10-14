namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped offboarding threshold is in its life cycle.</summary>
public enum CappedOffboardingThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A capped offboarding threshold as the offboarding team keeps it.</summary>
public sealed class CappedOffboardingThreshold(string code, string name, string label, double cost, int startsAt, CappedOffboardingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int StartsAt { get; set; } = startsAt;
    public CappedOffboardingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a capped offboarding threshold in status planned.</summary>
    public static CappedOffboardingThreshold NewCappedOffboardingThreshold(string code, string name)
    {
        return new CappedOffboardingThreshold(code, name, "", 0.0, 0, CappedOffboardingThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedOffboardingThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedOffboardingThresholdStatus next)
    {
        switch (Status)
        {
            case CappedOffboardingThresholdStatus.Planned:
                return next == CappedOffboardingThresholdStatus.Scheduled || next == CappedOffboardingThresholdStatus.InProgress;
            case CappedOffboardingThresholdStatus.Scheduled:
                switch (next)
                {
                    case CappedOffboardingThresholdStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedOffboardingThresholdStatus.InProgress:
                switch (next)
                {
                    case CappedOffboardingThresholdStatus.Completed:
                        return true;
                }
                return false;
            case CappedOffboardingThresholdStatus.Completed:
                switch (next)
                {
                    case CappedOffboardingThresholdStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedOffboardingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped offboarding threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Cost * (double)StartsAt, 2);
    }

    public CappedOffboardingThreshold Copy()
    {
        return new CappedOffboardingThreshold(Code, Name, Label, Cost, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
