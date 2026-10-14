namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched floor plan adjustment is in its life cycle.</summary>
public enum BatchedFloorPlanAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A batched floor plan adjustment. Its code is unique within a tenant.</summary>
public sealed class BatchedFloorPlanAdjustment(string code, string name, double rate, int startsAt, string reviewer, int createdAt, double ratio, string notes, string team, BatchedFloorPlanAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int StartsAt { get; set; } = startsAt;
    public string Reviewer { get; set; } = reviewer;
    public int CreatedAt { get; set; } = createdAt;
    public double Ratio { get; set; } = ratio;
    public string Notes { get; set; } = notes;
    public string Team { get; set; } = team;
    public BatchedFloorPlanAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a batched floor plan adjustment in status idle.</summary>
    public static BatchedFloorPlanAdjustment NewBatchedFloorPlanAdjustment(string code, string name)
    {
        return new BatchedFloorPlanAdjustment(
            code,
            name,
            0.0,
            0,
            "",
            0,
            0.0,
            "",
            "",
            BatchedFloorPlanAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedFloorPlanAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedFloorPlanAdjustmentStatus next)
    {
        switch (Status)
        {
            case BatchedFloorPlanAdjustmentStatus.Idle:
                return next == BatchedFloorPlanAdjustmentStatus.Ready || next == BatchedFloorPlanAdjustmentStatus.Draining;
            case BatchedFloorPlanAdjustmentStatus.Warming:
                switch (next)
                {
                    case BatchedFloorPlanAdjustmentStatus.Draining:
                    case BatchedFloorPlanAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedFloorPlanAdjustmentStatus.Ready:
                return next == BatchedFloorPlanAdjustmentStatus.Draining || next == BatchedFloorPlanAdjustmentStatus.Stopped;
            case BatchedFloorPlanAdjustmentStatus.Draining:
                return next == BatchedFloorPlanAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(BatchedFloorPlanAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched floor plan adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio + Rate, 2);
    }

    public BatchedFloorPlanAdjustment Copy()
    {
        return new BatchedFloorPlanAdjustment(
            Code,
            Name,
            Rate,
            StartsAt,
            Reviewer,
            CreatedAt,
            Ratio,
            Notes,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
