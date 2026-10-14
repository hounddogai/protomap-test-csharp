namespace Acme.Workforce.Compensation.BonusPool.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling bonus pool cycle is in its life cycle.</summary>
public enum RollingBonusPoolCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A rolling bonus pool cycle. Its code is unique within a tenant.</summary>
public sealed class RollingBonusPoolCycle(string code, string title, int durationMinutes, double rating, string timezone, double hours, double threshold, int priority, int revision, double minimum, RollingBonusPoolCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public double Hours { get; set; } = hours;
    public double Threshold { get; set; } = threshold;
    public int Priority { get; set; } = priority;
    public int Revision { get; set; } = revision;
    public double Minimum { get; set; } = minimum;
    public RollingBonusPoolCycleStatus Status { get; set; } = status;

    /// <summary>Starts a rolling bonus pool cycle in status requested.</summary>
    public static RollingBonusPoolCycle NewRollingBonusPoolCycle(string code, string title)
    {
        return new RollingBonusPoolCycle(
            code,
            title,
            0,
            0.0,
            "",
            0.0,
            0.0,
            0,
            0,
            0.0,
            RollingBonusPoolCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBonusPoolCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBonusPoolCycleStatus next)
    {
        switch (Status)
        {
            case RollingBonusPoolCycleStatus.Requested:
                return next == RollingBonusPoolCycleStatus.Granted;
            case RollingBonusPoolCycleStatus.Granted:
                switch (next)
                {
                    case RollingBonusPoolCycleStatus.Revoked:
                    case RollingBonusPoolCycleStatus.Expired:
                        return true;
                }
                return false;
            case RollingBonusPoolCycleStatus.Revoked:
                return next == RollingBonusPoolCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RollingBonusPoolCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling bonus pool cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"rolling bonus pool cycle"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold - Minimum, 2);
    }

    public RollingBonusPoolCycle Copy()
    {
        return new RollingBonusPoolCycle(
            Code,
            Title,
            DurationMinutes,
            Rating,
            Timezone,
            Hours,
            Threshold,
            Priority,
            Revision,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
