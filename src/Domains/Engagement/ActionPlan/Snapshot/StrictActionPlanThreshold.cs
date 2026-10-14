namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict action plan threshold is in its life cycle.</summary>
public enum StrictActionPlanThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a strict action plan threshold and its current status.</summary>
public sealed class StrictActionPlanThreshold(string code, string name, double rate, int quantity, string segment, int revision, string category, double threshold, StrictActionPlanThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public int Quantity { get; set; } = quantity;
    public string Segment { get; set; } = segment;
    public int Revision { get; set; } = revision;
    public string Category { get; set; } = category;
    public double Threshold { get; set; } = threshold;
    public StrictActionPlanThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a strict action plan threshold in status idle.</summary>
    public static StrictActionPlanThreshold NewStrictActionPlanThreshold(string code, string name)
    {
        return new StrictActionPlanThreshold(
            code,
            name,
            0.0,
            0,
            "",
            0,
            "",
            0.0,
            StrictActionPlanThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictActionPlanThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictActionPlanThresholdStatus next)
    {
        switch (Status)
        {
            case StrictActionPlanThresholdStatus.Idle:
                switch (next)
                {
                    case StrictActionPlanThresholdStatus.Warming:
                    case StrictActionPlanThresholdStatus.Ready:
                        return true;
                }
                return false;
            case StrictActionPlanThresholdStatus.Warming:
                return next == StrictActionPlanThresholdStatus.Draining;
            case StrictActionPlanThresholdStatus.Ready:
                switch (next)
                {
                    case StrictActionPlanThresholdStatus.Draining:
                    case StrictActionPlanThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case StrictActionPlanThresholdStatus.Draining:
                return next == StrictActionPlanThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictActionPlanThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict action plan threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"strict action plan threshold"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Threshold - Rate, 2);
    }

    public StrictActionPlanThreshold Copy()
    {
        return new StrictActionPlanThreshold(
            Code,
            Name,
            Rate,
            Quantity,
            Segment,
            Revision,
            Category,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
