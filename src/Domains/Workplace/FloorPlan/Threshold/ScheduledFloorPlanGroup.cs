namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled floor plan group is in its life cycle.</summary>
public enum ScheduledFloorPlanGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled floor plan group. Its code is unique within a tenant.</summary>
public sealed class ScheduledFloorPlanGroup(string code, string title, int version, bool visible, int graceDays, ScheduledFloorPlanGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public bool Visible { get; set; } = visible;
    public int GraceDays { get; set; } = graceDays;
    public ScheduledFloorPlanGroupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled floor plan group in status created.</summary>
    public static ScheduledFloorPlanGroup NewScheduledFloorPlanGroup(string code, string title)
    {
        return new ScheduledFloorPlanGroup(code, title, 0, false, 0, ScheduledFloorPlanGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFloorPlanGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFloorPlanGroupStatus next)
    {
        switch (Status)
        {
            case ScheduledFloorPlanGroupStatus.Created:
                return next == ScheduledFloorPlanGroupStatus.Validated || next == ScheduledFloorPlanGroupStatus.Published;
            case ScheduledFloorPlanGroupStatus.Validated:
                switch (next)
                {
                    case ScheduledFloorPlanGroupStatus.Published:
                    case ScheduledFloorPlanGroupStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledFloorPlanGroupStatus.Published:
                return next == ScheduledFloorPlanGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledFloorPlanGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled floor plan group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"scheduled floor plan group"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Version + (double)GraceDays, 2);
    }

    public ScheduledFloorPlanGroup Copy()
    {
        return new ScheduledFloorPlanGroup(Code, Title, Version, Visible, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
