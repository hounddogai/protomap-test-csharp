namespace Acme.Workforce.Workplace.Desk.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional desk cycle is in its life cycle.</summary>
public enum RegionalDeskCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional desk cycle as the desk team keeps it.</summary>
public sealed class RegionalDeskCycle(string code, string name, int limit, double cost, string location, double ratio, RegionalDeskCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public double Ratio { get; set; } = ratio;
    public RegionalDeskCycleStatus Status { get; set; } = status;

    /// <summary>Starts a regional desk cycle in status new.</summary>
    public static RegionalDeskCycle NewRegionalDeskCycle(string code, string name)
    {
        return new RegionalDeskCycle(code, name, 0, 0.0, "", 0.0, RegionalDeskCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalDeskCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalDeskCycleStatus next)
    {
        switch (Status)
        {
            case RegionalDeskCycleStatus.New:
                switch (next)
                {
                    case RegionalDeskCycleStatus.Running:
                    case RegionalDeskCycleStatus.Done:
                        return true;
                }
                return false;
            case RegionalDeskCycleStatus.Queued:
                switch (next)
                {
                    case RegionalDeskCycleStatus.Running:
                        return true;
                }
                return false;
            case RegionalDeskCycleStatus.Running:
                switch (next)
                {
                    case RegionalDeskCycleStatus.Done:
                    case RegionalDeskCycleStatus.Failed:
                        return true;
                }
                return false;
            case RegionalDeskCycleStatus.Done:
                return next == RegionalDeskCycleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalDeskCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional desk cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional desk cycle"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Cost * Ratio, 2);
    }

    public RegionalDeskCycle Copy()
    {
        return new RegionalDeskCycle(Code, Name, Limit, Cost, Location, Ratio, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
