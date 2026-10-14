namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual floor plan metric is in its life cycle.</summary>
public enum ManualFloorPlanMetricStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual floor plan metric as the floor plan team keeps it.</summary>
public sealed class ManualFloorPlanMetric(string code, string label, double weight, string title, double score, ManualFloorPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public ManualFloorPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a manual floor plan metric in status created.</summary>
    public static ManualFloorPlanMetric NewManualFloorPlanMetric(string code, string label)
    {
        return new ManualFloorPlanMetric(code, label, 0.0, "", 0.0, ManualFloorPlanMetricStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualFloorPlanMetricStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualFloorPlanMetricStatus next)
    {
        switch (Status)
        {
            case ManualFloorPlanMetricStatus.Created:
                return next == ManualFloorPlanMetricStatus.Validated || next == ManualFloorPlanMetricStatus.Published;
            case ManualFloorPlanMetricStatus.Validated:
                switch (next)
                {
                    case ManualFloorPlanMetricStatus.Retired:
                        return true;
                }
                return false;
            case ManualFloorPlanMetricStatus.Published:
                return next == ManualFloorPlanMetricStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualFloorPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual floor plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetValue()
    {
        return Math.Round(Weight - Score, 2);
    }

    public ManualFloorPlanMetric Copy()
    {
        return new ManualFloorPlanMetric(Code, Label, Weight, Title, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
