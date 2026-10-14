namespace Acme.Workforce.Payrollops.Garnishment.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment cycle is in its life cycle.</summary>
public enum GarnishmentCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A garnishment cycle as the garnishment team keeps it.</summary>
public sealed class GarnishmentCycle(string code, string name, string jobCode, string unit, int priority, bool approved, string summary, string team, int tenureMonths, GarnishmentCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Unit { get; set; } = unit;
    public int Priority { get; set; } = priority;
    public bool Approved { get; set; } = approved;
    public string Summary { get; set; } = summary;
    public string Team { get; set; } = team;
    public int TenureMonths { get; set; } = tenureMonths;
    public GarnishmentCycleStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment cycle in status planned.</summary>
    public static GarnishmentCycle NewGarnishmentCycle(string code, string name)
    {
        return new GarnishmentCycle(code, name, "", "", 0, false, "", "", 0, GarnishmentCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentCycleStatus next)
    {
        switch (Status)
        {
            case GarnishmentCycleStatus.Planned:
                return next == GarnishmentCycleStatus.InProgress || next == GarnishmentCycleStatus.Cancelled;
            case GarnishmentCycleStatus.Scheduled:
                switch (next)
                {
                    case GarnishmentCycleStatus.Completed:
                        return true;
                }
                return false;
            case GarnishmentCycleStatus.InProgress:
                switch (next)
                {
                    case GarnishmentCycleStatus.Completed:
                    case GarnishmentCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case GarnishmentCycleStatus.Completed:
                switch (next)
                {
                    case GarnishmentCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"garnishment cycle"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Priority - (double)TenureMonths, 2);
    }

    public GarnishmentCycle Copy()
    {
        return new GarnishmentCycle(
            Code,
            Name,
            JobCode,
            Unit,
            Priority,
            Approved,
            Summary,
            Team,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
