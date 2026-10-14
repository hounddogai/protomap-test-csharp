namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan score is in its life cycle.</summary>
public enum FloorPlanScoreStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A floor plan score as the floor plan team keeps it.</summary>
public sealed class FloorPlanScore(string code, string name, string checksum, int createdAt, string salaryBand, string managerId, FloorPlanScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public int CreatedAt { get; set; } = createdAt;
    public string SalaryBand { get; set; } = salaryBand;
    public string ManagerId { get; set; } = managerId;
    public FloorPlanScoreStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan score in status idle.</summary>
    public static FloorPlanScore NewFloorPlanScore(string code, string name)
    {
        return new FloorPlanScore(code, name, "", 0, "", "", FloorPlanScoreStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanScoreStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanScoreStatus next)
    {
        switch (Status)
        {
            case FloorPlanScoreStatus.Idle:
                return next == FloorPlanScoreStatus.Draining || next == FloorPlanScoreStatus.Stopped;
            case FloorPlanScoreStatus.Warming:
                return next == FloorPlanScoreStatus.Draining || next == FloorPlanScoreStatus.Stopped;
            case FloorPlanScoreStatus.Ready:
                return next == FloorPlanScoreStatus.Draining || next == FloorPlanScoreStatus.Stopped;
            case FloorPlanScoreStatus.Draining:
                return next == FloorPlanScoreStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FloorPlanScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public FloorPlanScore Copy()
    {
        return new FloorPlanScore(Code, Name, Checksum, CreatedAt, SalaryBand, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
