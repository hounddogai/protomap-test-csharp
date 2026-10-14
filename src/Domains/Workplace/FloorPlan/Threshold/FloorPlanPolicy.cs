namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan policy is in its life cycle.</summary>
public enum FloorPlanPolicyStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a floor plan policy and its current status.</summary>
public sealed class FloorPlanPolicy(string code, string title, string description, int windowDays, double budget, int retries, bool visible, string managerId, FloorPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public bool Visible { get; set; } = visible;
    public string ManagerId { get; set; } = managerId;
    public FloorPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan policy in status idle.</summary>
    public static FloorPlanPolicy NewFloorPlanPolicy(string code, string title)
    {
        return new FloorPlanPolicy(code, title, "", 0, 0.0, 0, false, "", FloorPlanPolicyStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanPolicyStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanPolicyStatus next)
    {
        switch (Status)
        {
            case FloorPlanPolicyStatus.Idle:
                return next == FloorPlanPolicyStatus.Warming || next == FloorPlanPolicyStatus.Ready;
            case FloorPlanPolicyStatus.Warming:
                switch (next)
                {
                    case FloorPlanPolicyStatus.Draining:
                        return true;
                }
                return false;
            case FloorPlanPolicyStatus.Ready:
                return next == FloorPlanPolicyStatus.Stopped;
            case FloorPlanPolicyStatus.Draining:
                return next == FloorPlanPolicyStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FloorPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)WindowDays - Budget, 2);
    }

    public FloorPlanPolicy Copy()
    {
        return new FloorPlanPolicy(
            Code,
            Title,
            Description,
            WindowDays,
            Budget,
            Retries,
            Visible,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
