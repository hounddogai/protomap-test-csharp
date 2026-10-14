namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered headcount estimate is in its life cycle.</summary>
public enum TieredHeadcountEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered headcount estimate. Its code is unique within a tenant.</summary>
public sealed class TieredHeadcountEstimate(string code, string name, int version, string source, bool active, TieredHeadcountEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Source { get; set; } = source;
    public bool Active { get; set; } = active;
    public TieredHeadcountEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a tiered headcount estimate in status planned.</summary>
    public static TieredHeadcountEstimate NewTieredHeadcountEstimate(string code, string name)
    {
        return new TieredHeadcountEstimate(code, name, 0, "", false, TieredHeadcountEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredHeadcountEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredHeadcountEstimateStatus next)
    {
        switch (Status)
        {
            case TieredHeadcountEstimateStatus.Planned:
                return next == TieredHeadcountEstimateStatus.Completed;
            case TieredHeadcountEstimateStatus.Scheduled:
                switch (next)
                {
                    case TieredHeadcountEstimateStatus.InProgress:
                    case TieredHeadcountEstimateStatus.Completed:
                        return true;
                }
                return false;
            case TieredHeadcountEstimateStatus.InProgress:
                switch (next)
                {
                    case TieredHeadcountEstimateStatus.Completed:
                    case TieredHeadcountEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredHeadcountEstimateStatus.Completed:
                switch (next)
                {
                    case TieredHeadcountEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredHeadcountEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered headcount estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"tiered headcount estimate"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public TieredHeadcountEstimate Copy()
    {
        return new TieredHeadcountEstimate(Code, Name, Version, Source, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
