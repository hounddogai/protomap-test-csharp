namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict floor plan bundle is in its life cycle.</summary>
public enum StrictFloorPlanBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a strict floor plan bundle and its current status.</summary>
public sealed class StrictFloorPlanBundle(string code, string name, int startsAt, string unit, double floorValue, string salaryBand, bool approved, StrictFloorPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public string Unit { get; set; } = unit;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Approved { get; set; } = approved;
    public StrictFloorPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a strict floor plan bundle in status planned.</summary>
    public static StrictFloorPlanBundle NewStrictFloorPlanBundle(string code, string name)
    {
        return new StrictFloorPlanBundle(code, name, 0, "", 0.0, "", false, StrictFloorPlanBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictFloorPlanBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictFloorPlanBundleStatus next)
    {
        switch (Status)
        {
            case StrictFloorPlanBundleStatus.Planned:
                switch (next)
                {
                    case StrictFloorPlanBundleStatus.InProgress:
                        return true;
                }
                return false;
            case StrictFloorPlanBundleStatus.Scheduled:
                switch (next)
                {
                    case StrictFloorPlanBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictFloorPlanBundleStatus.InProgress:
                return next == StrictFloorPlanBundleStatus.Cancelled;
            case StrictFloorPlanBundleStatus.Completed:
                return next == StrictFloorPlanBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StrictFloorPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict floor plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)StartsAt + FloorValue, 2);
    }

    public StrictFloorPlanBundle Copy()
    {
        return new StrictFloorPlanBundle(Code, Name, StartsAt, Unit, FloorValue, SalaryBand, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
