namespace Acme.Workforce.Workplace.Visitor.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor cycle is in its life cycle.</summary>
public enum VisitorCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One visitor cycle, with what the planners need to act on it.</summary>
public sealed class VisitorCycle(string code, string name, double cost, double hours, int updatedAt, int tenureMonths, double maximum, string location, int createdAt, int version, VisitorCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public double Hours { get; set; } = hours;
    public int UpdatedAt { get; set; } = updatedAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public int CreatedAt { get; set; } = createdAt;
    public int Version { get; set; } = version;
    public VisitorCycleStatus Status { get; set; } = status;

    /// <summary>Starts a visitor cycle in status idle.</summary>
    public static VisitorCycle NewVisitorCycle(string code, string name)
    {
        return new VisitorCycle(code, name, 0.0, 0.0, 0, 0, 0.0, "", 0, 0, VisitorCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorCycleStatus next)
    {
        switch (Status)
        {
            case VisitorCycleStatus.Idle:
                return next == VisitorCycleStatus.Warming || next == VisitorCycleStatus.Ready;
            case VisitorCycleStatus.Warming:
                switch (next)
                {
                    case VisitorCycleStatus.Ready:
                    case VisitorCycleStatus.Draining:
                        return true;
                }
                return false;
            case VisitorCycleStatus.Ready:
                switch (next)
                {
                    case VisitorCycleStatus.Draining:
                        return true;
                }
                return false;
            case VisitorCycleStatus.Draining:
                switch (next)
                {
                    case VisitorCycleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost + (double)CreatedAt, 2);
    }

    public VisitorCycle Copy()
    {
        return new VisitorCycle(
            Code,
            Name,
            Cost,
            Hours,
            UpdatedAt,
            TenureMonths,
            Maximum,
            Location,
            CreatedAt,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
