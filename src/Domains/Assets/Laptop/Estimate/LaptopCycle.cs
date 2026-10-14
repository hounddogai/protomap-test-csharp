namespace Acme.Workforce.Assets.Laptop.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop cycle is in its life cycle.</summary>
public enum LaptopCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One laptop cycle, with what the planners need to act on it.</summary>
public sealed class LaptopCycle(string code, string name, double threshold, string department, int leaveDays, string summary, double margin, int capacity, double hours, int limit, int expiresAt, LaptopCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Department { get; set; } = department;
    public int LeaveDays { get; set; } = leaveDays;
    public string Summary { get; set; } = summary;
    public double Margin { get; set; } = margin;
    public int Capacity { get; set; } = capacity;
    public double Hours { get; set; } = hours;
    public int Limit { get; set; } = limit;
    public int ExpiresAt { get; set; } = expiresAt;
    public LaptopCycleStatus Status { get; set; } = status;

    /// <summary>Starts a laptop cycle in status requested.</summary>
    public static LaptopCycle NewLaptopCycle(string code, string name)
    {
        return new LaptopCycle(code, name, 0.0, "", 0, "", 0.0, 0, 0.0, 0, 0, LaptopCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopCycleStatus next)
    {
        switch (Status)
        {
            case LaptopCycleStatus.Requested:
                switch (next)
                {
                    case LaptopCycleStatus.Revoked:
                    case LaptopCycleStatus.Expired:
                        return true;
                }
                return false;
            case LaptopCycleStatus.Granted:
                return next == LaptopCycleStatus.Expired;
            case LaptopCycleStatus.Revoked:
                switch (next)
                {
                    case LaptopCycleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LaptopCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Capacity + (double)Limit, 2);
    }

    public LaptopCycle Copy()
    {
        return new LaptopCycle(
            Code,
            Name,
            Threshold,
            Department,
            LeaveDays,
            Summary,
            Margin,
            Capacity,
            Hours,
            Limit,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
