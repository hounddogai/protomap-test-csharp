namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback merit cycle checkpoint is in its life cycle.</summary>
public enum FallbackMeritCycleCheckpointStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A fallback merit cycle checkpoint as the merit cycle team keeps it.</summary>
public sealed class FallbackMeritCycleCheckpoint(string code, string name, double floorValue, int leaveDays, bool approved, int limit, FallbackMeritCycleCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Approved { get; set; } = approved;
    public int Limit { get; set; } = limit;
    public FallbackMeritCycleCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a fallback merit cycle checkpoint in status draft.</summary>
    public static FallbackMeritCycleCheckpoint NewFallbackMeritCycleCheckpoint(string code, string name)
    {
        return new FallbackMeritCycleCheckpoint(
            code,
            name,
            0.0,
            0,
            false,
            0,
            FallbackMeritCycleCheckpointStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackMeritCycleCheckpointStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackMeritCycleCheckpointStatus next)
    {
        switch (Status)
        {
            case FallbackMeritCycleCheckpointStatus.Draft:
                switch (next)
                {
                    case FallbackMeritCycleCheckpointStatus.Pending:
                    case FallbackMeritCycleCheckpointStatus.Closed:
                        return true;
                }
                return false;
            case FallbackMeritCycleCheckpointStatus.Pending:
                switch (next)
                {
                    case FallbackMeritCycleCheckpointStatus.Active:
                    case FallbackMeritCycleCheckpointStatus.Suspended:
                        return true;
                }
                return false;
            case FallbackMeritCycleCheckpointStatus.Active:
                return next == FallbackMeritCycleCheckpointStatus.Closed;
            case FallbackMeritCycleCheckpointStatus.Suspended:
                switch (next)
                {
                    case FallbackMeritCycleCheckpointStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackMeritCycleCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback merit cycle checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Limit - FloorValue, 2);
    }

    public FallbackMeritCycleCheckpoint Copy()
    {
        return new FallbackMeritCycleCheckpoint(Code, Name, FloorValue, LeaveDays, Approved, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
