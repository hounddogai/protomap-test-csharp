namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive position plan is in its life cycle.</summary>
public enum AdaptivePositionPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adaptive position plan. Its code is unique within a tenant.</summary>
public sealed class AdaptivePositionPlan(string code, string name, int quantity, int version, double floorValue, bool pinned, int limit, double threshold, double score, int retries, AdaptivePositionPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public bool Pinned { get; set; } = pinned;
    public int Limit { get; set; } = limit;
    public double Threshold { get; set; } = threshold;
    public double Score { get; set; } = score;
    public int Retries { get; set; } = retries;
    public AdaptivePositionPlanStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive position plan in status idle.</summary>
    public static AdaptivePositionPlan NewAdaptivePositionPlan(string code, string name)
    {
        return new AdaptivePositionPlan(
            code,
            name,
            0,
            0,
            0.0,
            false,
            0,
            0.0,
            0.0,
            0,
            AdaptivePositionPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePositionPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePositionPlanStatus next)
    {
        switch (Status)
        {
            case AdaptivePositionPlanStatus.Idle:
                return next == AdaptivePositionPlanStatus.Warming || next == AdaptivePositionPlanStatus.Stopped;
            case AdaptivePositionPlanStatus.Warming:
                return next == AdaptivePositionPlanStatus.Draining || next == AdaptivePositionPlanStatus.Stopped;
            case AdaptivePositionPlanStatus.Ready:
                return next == AdaptivePositionPlanStatus.Draining || next == AdaptivePositionPlanStatus.Stopped;
            case AdaptivePositionPlanStatus.Draining:
                return next == AdaptivePositionPlanStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptivePositionPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive position plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Threshold * Score, 2);
    }

    public AdaptivePositionPlan Copy()
    {
        return new AdaptivePositionPlan(
            Code,
            Name,
            Quantity,
            Version,
            FloorValue,
            Pinned,
            Limit,
            Threshold,
            Score,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
