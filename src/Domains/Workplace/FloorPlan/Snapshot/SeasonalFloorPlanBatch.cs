namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal floor plan batch is in its life cycle.</summary>
public enum SeasonalFloorPlanBatchStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One seasonal floor plan batch, with what the planners need to act on it.</summary>
public sealed class SeasonalFloorPlanBatch(string code, string name, double floorValue, bool active, int leaveDays, double amount, SeasonalFloorPlanBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public bool Active { get; set; } = active;
    public int LeaveDays { get; set; } = leaveDays;
    public double Amount { get; set; } = amount;
    public SeasonalFloorPlanBatchStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal floor plan batch in status requested.</summary>
    public static SeasonalFloorPlanBatch NewSeasonalFloorPlanBatch(string code, string name)
    {
        return new SeasonalFloorPlanBatch(code, name, 0.0, false, 0, 0.0, SeasonalFloorPlanBatchStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFloorPlanBatchStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFloorPlanBatchStatus next)
    {
        switch (Status)
        {
            case SeasonalFloorPlanBatchStatus.Requested:
                switch (next)
                {
                    case SeasonalFloorPlanBatchStatus.Granted:
                        return true;
                }
                return false;
            case SeasonalFloorPlanBatchStatus.Granted:
                return next == SeasonalFloorPlanBatchStatus.Revoked || next == SeasonalFloorPlanBatchStatus.Expired;
            case SeasonalFloorPlanBatchStatus.Revoked:
                return next == SeasonalFloorPlanBatchStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalFloorPlanBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal floor plan batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"seasonal floor plan batch"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Amount * (double)LeaveDays, 2);
    }

    public SeasonalFloorPlanBatch Copy()
    {
        return new SeasonalFloorPlanBatch(Code, Name, FloorValue, Active, LeaveDays, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
