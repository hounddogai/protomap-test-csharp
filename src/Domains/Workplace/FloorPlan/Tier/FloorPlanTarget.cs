namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan target is in its life cycle.</summary>
public enum FloorPlanTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a floor plan target and its current status.</summary>
public sealed class FloorPlanTarget(string code, string title, string jobCode, int graceDays, string name, double amount, string managerId, FloorPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public FloorPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan target in status created.</summary>
    public static FloorPlanTarget NewFloorPlanTarget(string code, string title)
    {
        return new FloorPlanTarget(code, title, "", 0, "", 0.0, "", FloorPlanTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanTargetStatus next)
    {
        switch (Status)
        {
            case FloorPlanTargetStatus.Created:
                switch (next)
                {
                    case FloorPlanTargetStatus.Published:
                    case FloorPlanTargetStatus.Retired:
                        return true;
                }
                return false;
            case FloorPlanTargetStatus.Validated:
                switch (next)
                {
                    case FloorPlanTargetStatus.Retired:
                        return true;
                }
                return false;
            case FloorPlanTargetStatus.Published:
                return next == FloorPlanTargetStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FloorPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"floor plan target"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)GraceDays - Amount, 2);
    }

    public FloorPlanTarget Copy()
    {
        return new FloorPlanTarget(Code, Title, JobCode, GraceDays, Name, Amount, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
