namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive floor plan metric is in its life cycle.</summary>
public enum AdaptiveFloorPlanMetricStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive floor plan metric, with what the planners need to act on it.</summary>
public sealed class AdaptiveFloorPlanMetric(string code, string label, double rating, double ratio, List<string> tags, AdaptiveFloorPlanMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public AdaptiveFloorPlanMetricStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive floor plan metric in status new.</summary>
    public static AdaptiveFloorPlanMetric NewAdaptiveFloorPlanMetric(string code, string label)
    {
        return new AdaptiveFloorPlanMetric(
            code,
            label,
            0.0,
            0.0,
            new List<string>(),
            AdaptiveFloorPlanMetricStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFloorPlanMetricStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFloorPlanMetricStatus next)
    {
        switch (Status)
        {
            case AdaptiveFloorPlanMetricStatus.New:
                return next == AdaptiveFloorPlanMetricStatus.Running || next == AdaptiveFloorPlanMetricStatus.Failed;
            case AdaptiveFloorPlanMetricStatus.Queued:
                return next == AdaptiveFloorPlanMetricStatus.Running;
            case AdaptiveFloorPlanMetricStatus.Running:
                switch (next)
                {
                    case AdaptiveFloorPlanMetricStatus.Done:
                    case AdaptiveFloorPlanMetricStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveFloorPlanMetricStatus.Done:
                return next == AdaptiveFloorPlanMetricStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveFloorPlanMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive floor plan metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double EffectiveExposure()
    {
        return Math.Round(Rating * Ratio, 2);
    }

    public AdaptiveFloorPlanMetric Copy()
    {
        return new AdaptiveFloorPlanMetric(Code, Label, Rating, Ratio, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
