namespace Acme.Workforce.Orgdesign.Headcount.HeadcountSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered headcount schedule is in its life cycle.</summary>
public enum TieredHeadcountScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One tiered headcount schedule, with what the planners need to act on it.</summary>
public sealed class TieredHeadcountSchedule(string code, string name, string channel, string category, double hours, string timezone, string jobCode, double rate, int tenureMonths, int expiresAt, TieredHeadcountScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string Category { get; set; } = category;
    public double Hours { get; set; } = hours;
    public string Timezone { get; set; } = timezone;
    public string JobCode { get; set; } = jobCode;
    public double Rate { get; set; } = rate;
    public int TenureMonths { get; set; } = tenureMonths;
    public int ExpiresAt { get; set; } = expiresAt;
    public TieredHeadcountScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered headcount schedule in status planned.</summary>
    public static TieredHeadcountSchedule NewTieredHeadcountSchedule(string code, string name)
    {
        return new TieredHeadcountSchedule(
            code,
            name,
            "",
            "",
            0.0,
            "",
            "",
            0.0,
            0,
            0,
            TieredHeadcountScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredHeadcountScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredHeadcountScheduleStatus next)
    {
        switch (Status)
        {
            case TieredHeadcountScheduleStatus.Planned:
                switch (next)
                {
                    case TieredHeadcountScheduleStatus.InProgress:
                        return true;
                }
                return false;
            case TieredHeadcountScheduleStatus.Scheduled:
                switch (next)
                {
                    case TieredHeadcountScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredHeadcountScheduleStatus.InProgress:
                return next == TieredHeadcountScheduleStatus.Completed || next == TieredHeadcountScheduleStatus.Cancelled;
            case TieredHeadcountScheduleStatus.Completed:
                switch (next)
                {
                    case TieredHeadcountScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredHeadcountScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered headcount schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered headcount schedule"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)ExpiresAt + Hours, 2);
    }

    public TieredHeadcountSchedule Copy()
    {
        return new TieredHeadcountSchedule(
            Code,
            Name,
            Channel,
            Category,
            Hours,
            Timezone,
            JobCode,
            Rate,
            TenureMonths,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
