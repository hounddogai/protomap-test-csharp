namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan metric is in its life cycle.</summary>
public enum FloorPlanMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a floor plan metric and its current status.</summary>
public sealed class FloorPlanMetric(string code, string name, double ceiling, bool visible, string title, double rate, FloorPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public bool Visible { get; set; } = visible;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public FloorPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan metric in status created.</summary>
    public static FloorPlanMetric NewFloorPlanMetric(string code, string name)
    {
        return new FloorPlanMetric(code, name, 0.0, false, "", 0.0, FloorPlanMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanMetricStatus next)
    {
        switch (Status)
        {
            case FloorPlanMetricStatus.Created:
                switch (next)
                {
                    case FloorPlanMetricStatus.Retired:
                        return true;
                }
                return false;
            case FloorPlanMetricStatus.Validated:
                return next == FloorPlanMetricStatus.Published;
            case FloorPlanMetricStatus.Published:
                switch (next)
                {
                    case FloorPlanMetricStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Ceiling + Rate, 2);
    }

    public FloorPlanMetric Copy()
    {
        return new FloorPlanMetric(Code, Name, Ceiling, Visible, Title, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
