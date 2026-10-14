namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pool cycle is in its life cycle.</summary>
public enum GlobalPoolCycleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a global pool cycle and its current status.</summary>
public sealed class GlobalPoolCycle(string code, string name, string employeeId, string timezone, double weight, double rating, string unit, string checksum, bool approved, GlobalPoolCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string Timezone { get; set; } = timezone;
    public double Weight { get; set; } = weight;
    public double Rating { get; set; } = rating;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public bool Approved { get; set; } = approved;
    public GlobalPoolCycleStatus Status { get; set; } = status;

    /// <summary>Starts a global pool cycle in status new.</summary>
    public static GlobalPoolCycle NewGlobalPoolCycle(string code, string name)
    {
        return new GlobalPoolCycle(code, name, "", "", 0.0, 0.0, "", "", false, GlobalPoolCycleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPoolCycleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPoolCycleStatus next)
    {
        switch (Status)
        {
            case GlobalPoolCycleStatus.New:
                return next == GlobalPoolCycleStatus.Queued || next == GlobalPoolCycleStatus.Running;
            case GlobalPoolCycleStatus.Queued:
                switch (next)
                {
                    case GlobalPoolCycleStatus.Done:
                        return true;
                }
                return false;
            case GlobalPoolCycleStatus.Running:
                switch (next)
                {
                    case GlobalPoolCycleStatus.Done:
                        return true;
                }
                return false;
            case GlobalPoolCycleStatus.Done:
                switch (next)
                {
                    case GlobalPoolCycleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPoolCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pool cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Weight * Rating, 2);
    }

    public GlobalPoolCycle Copy()
    {
        return new GlobalPoolCycle(
            Code,
            Name,
            EmployeeId,
            Timezone,
            Weight,
            Rating,
            Unit,
            Checksum,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
