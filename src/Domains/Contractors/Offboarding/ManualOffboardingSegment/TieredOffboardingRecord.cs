namespace Acme.Workforce.Contractors.Offboarding.ManualOffboardingSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered offboarding record is in its life cycle.</summary>
public enum TieredOffboardingRecordStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered offboarding record as the offboarding team keeps it.</summary>
public sealed class TieredOffboardingRecord(string code, string name, bool archived, double amount, string region, double ratio, TieredOffboardingRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public string Region { get; set; } = region;
    public double Ratio { get; set; } = ratio;
    public TieredOffboardingRecordStatus Status { get; set; } = status;

    /// <summary>Starts a tiered offboarding record in status planned.</summary>
    public static TieredOffboardingRecord NewTieredOffboardingRecord(string code, string name)
    {
        return new TieredOffboardingRecord(code, name, false, 0.0, "", 0.0, TieredOffboardingRecordStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredOffboardingRecordStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredOffboardingRecordStatus next)
    {
        switch (Status)
        {
            case TieredOffboardingRecordStatus.Planned:
                return next == TieredOffboardingRecordStatus.Completed;
            case TieredOffboardingRecordStatus.Scheduled:
                return next == TieredOffboardingRecordStatus.InProgress;
            case TieredOffboardingRecordStatus.InProgress:
                return next == TieredOffboardingRecordStatus.Cancelled;
            case TieredOffboardingRecordStatus.Completed:
                return next == TieredOffboardingRecordStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredOffboardingRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered offboarding record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio - Amount, 2);
    }

    public TieredOffboardingRecord Copy()
    {
        return new TieredOffboardingRecord(Code, Name, Archived, Amount, Region, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
