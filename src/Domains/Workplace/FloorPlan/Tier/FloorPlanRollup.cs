namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan rollup is in its life cycle.</summary>
public enum FloorPlanRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A floor plan rollup. Its code is unique within a tenant.</summary>
public sealed class FloorPlanRollup(string code, string name, string jobCode, string salaryBand, double fte, bool visible, FloorPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string SalaryBand { get; set; } = salaryBand;
    public double Fte { get; set; } = fte;
    public bool Visible { get; set; } = visible;
    public FloorPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan rollup in status new.</summary>
    public static FloorPlanRollup NewFloorPlanRollup(string code, string name)
    {
        return new FloorPlanRollup(code, name, "", "", 0.0, false, FloorPlanRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanRollupStatus next)
    {
        switch (Status)
        {
            case FloorPlanRollupStatus.New:
                switch (next)
                {
                    case FloorPlanRollupStatus.Running:
                    case FloorPlanRollupStatus.Done:
                        return true;
                }
                return false;
            case FloorPlanRollupStatus.Queued:
                switch (next)
                {
                    case FloorPlanRollupStatus.Running:
                    case FloorPlanRollupStatus.Done:
                        return true;
                }
                return false;
            case FloorPlanRollupStatus.Running:
                switch (next)
                {
                    case FloorPlanRollupStatus.Failed:
                        return true;
                }
                return false;
            case FloorPlanRollupStatus.Done:
                return next == FloorPlanRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(FloorPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public FloorPlanRollup Copy()
    {
        return new FloorPlanRollup(Code, Name, JobCode, SalaryBand, Fte, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
