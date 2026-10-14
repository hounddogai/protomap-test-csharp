namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback floor plan metric is in its life cycle.</summary>
public enum FallbackFloorPlanMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback floor plan metric. Its code is unique within a tenant.</summary>
public sealed class FallbackFloorPlanMetric(string code, string label, List<string> tags, string checksum, int attempts, double margin, int leaveDays, FallbackFloorPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public FallbackFloorPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a fallback floor plan metric in status idle.</summary>
    public static FallbackFloorPlanMetric NewFallbackFloorPlanMetric(string code, string label)
    {
        return new FallbackFloorPlanMetric(
            code,
            label,
            new List<string>(),
            "",
            0,
            0.0,
            0,
            FallbackFloorPlanMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackFloorPlanMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackFloorPlanMetricStatus next)
    {
        switch (Status)
        {
            case FallbackFloorPlanMetricStatus.Idle:
                return next == FallbackFloorPlanMetricStatus.Warming || next == FallbackFloorPlanMetricStatus.Ready;
            case FallbackFloorPlanMetricStatus.Warming:
                return next == FallbackFloorPlanMetricStatus.Ready || next == FallbackFloorPlanMetricStatus.Stopped;
            case FallbackFloorPlanMetricStatus.Ready:
                switch (next)
                {
                    case FallbackFloorPlanMetricStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackFloorPlanMetricStatus.Draining:
                return next == FallbackFloorPlanMetricStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackFloorPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback floor plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetVolume()
    {
        return Math.Round(Margin * (double)LeaveDays, 2);
    }

    public FallbackFloorPlanMetric Copy()
    {
        return new FallbackFloorPlanMetric(Code, Label, Tags, Checksum, Attempts, Margin, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
