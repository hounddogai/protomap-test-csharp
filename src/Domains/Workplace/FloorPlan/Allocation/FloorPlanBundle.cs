namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan bundle is in its life cycle.</summary>
public enum FloorPlanBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One floor plan bundle, with what the planners need to act on it.</summary>
public sealed class FloorPlanBundle(string code, string title, double minimum, double floorValue, string salaryBand, string unit, bool pinned, double cost, string jobCode, FloorPlanBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public string JobCode { get; set; } = jobCode;
    public FloorPlanBundleStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan bundle in status new.</summary>
    public static FloorPlanBundle NewFloorPlanBundle(string code, string title)
    {
        return new FloorPlanBundle(code, title, 0.0, 0.0, "", "", false, 0.0, "", FloorPlanBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanBundleStatus next)
    {
        switch (Status)
        {
            case FloorPlanBundleStatus.New:
                switch (next)
                {
                    case FloorPlanBundleStatus.Done:
                    case FloorPlanBundleStatus.Failed:
                        return true;
                }
                return false;
            case FloorPlanBundleStatus.Queued:
                return next == FloorPlanBundleStatus.Failed;
            case FloorPlanBundleStatus.Running:
                switch (next)
                {
                    case FloorPlanBundleStatus.Done:
                    case FloorPlanBundleStatus.Failed:
                        return true;
                }
                return false;
            case FloorPlanBundleStatus.Done:
                switch (next)
                {
                    case FloorPlanBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(FloorValue - Minimum, 2);
    }

    public FloorPlanBundle Copy()
    {
        return new FloorPlanBundle(
            Code,
            Title,
            Minimum,
            FloorValue,
            SalaryBand,
            Unit,
            Pinned,
            Cost,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
