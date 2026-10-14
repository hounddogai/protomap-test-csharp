namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan override is in its life cycle.</summary>
public enum FloorPlanOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A floor plan override. Its code is unique within a tenant.</summary>
public sealed class FloorPlanOverride(string code, string label, int version, double minimum, int retries, string channel, FloorPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public string Channel { get; set; } = channel;
    public FloorPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan override in status planned.</summary>
    public static FloorPlanOverride NewFloorPlanOverride(string code, string label)
    {
        return new FloorPlanOverride(code, label, 0, 0.0, 0, "", FloorPlanOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanOverrideStatus next)
    {
        switch (Status)
        {
            case FloorPlanOverrideStatus.Planned:
                return next == FloorPlanOverrideStatus.Scheduled;
            case FloorPlanOverrideStatus.Scheduled:
                return next == FloorPlanOverrideStatus.Completed;
            case FloorPlanOverrideStatus.InProgress:
                switch (next)
                {
                    case FloorPlanOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanOverrideStatus.Completed:
                switch (next)
                {
                    case FloorPlanOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Version - Minimum, 2);
    }

    public FloorPlanOverride Copy()
    {
        return new FloorPlanOverride(Code, Label, Version, Minimum, Retries, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
