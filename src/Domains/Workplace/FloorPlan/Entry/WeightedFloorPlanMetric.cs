namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted floor plan metric is in its life cycle.</summary>
public enum WeightedFloorPlanMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted floor plan metric. Its code is unique within a tenant.</summary>
public sealed class WeightedFloorPlanMetric(string code, string label, string phone, double margin, string externalRef, string salaryBand, double ceiling, string channel, WeightedFloorPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Margin { get; set; } = margin;
    public string ExternalRef { get; set; } = externalRef;
    public string SalaryBand { get; set; } = salaryBand;
    public double Ceiling { get; set; } = ceiling;
    public string Channel { get; set; } = channel;
    public WeightedFloorPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a weighted floor plan metric in status proposed.</summary>
    public static WeightedFloorPlanMetric NewWeightedFloorPlanMetric(string code, string label)
    {
        return new WeightedFloorPlanMetric(
            code,
            label,
            "",
            0.0,
            "",
            "",
            0.0,
            "",
            WeightedFloorPlanMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedFloorPlanMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedFloorPlanMetricStatus next)
    {
        switch (Status)
        {
            case WeightedFloorPlanMetricStatus.Proposed:
                switch (next)
                {
                    case WeightedFloorPlanMetricStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedFloorPlanMetricStatus.Approved:
                return next == WeightedFloorPlanMetricStatus.Rejected || next == WeightedFloorPlanMetricStatus.Withdrawn;
            case WeightedFloorPlanMetricStatus.Rejected:
                return next == WeightedFloorPlanMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WeightedFloorPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted floor plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted floor plan metric"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Ceiling * Margin, 2);
    }

    public WeightedFloorPlanMetric Copy()
    {
        return new WeightedFloorPlanMetric(
            Code,
            Label,
            Phone,
            Margin,
            ExternalRef,
            SalaryBand,
            Ceiling,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
