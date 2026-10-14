namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge threshold is in its life cycle.</summary>
public enum AccessBadgeThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One access badge threshold, with what the planners need to act on it.</summary>
public sealed class AccessBadgeThreshold(string code, string name, string unit, string region, double floorValue, string badge, int capacity, string channel, bool archived, AccessBadgeThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string Region { get; set; } = region;
    public double FloorValue { get; set; } = floorValue;
    public string Badge { get; set; } = badge;
    public int Capacity { get; set; } = capacity;
    public string Channel { get; set; } = channel;
    public bool Archived { get; set; } = archived;
    public AccessBadgeThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a access badge threshold in status idle.</summary>
    public static AccessBadgeThreshold NewAccessBadgeThreshold(string code, string name)
    {
        return new AccessBadgeThreshold(code, name, "", "", 0.0, "", 0, "", false, AccessBadgeThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeThresholdStatus next)
    {
        switch (Status)
        {
            case AccessBadgeThresholdStatus.Idle:
                return next == AccessBadgeThresholdStatus.Warming || next == AccessBadgeThresholdStatus.Draining;
            case AccessBadgeThresholdStatus.Warming:
                return next == AccessBadgeThresholdStatus.Ready || next == AccessBadgeThresholdStatus.Stopped;
            case AccessBadgeThresholdStatus.Ready:
                switch (next)
                {
                    case AccessBadgeThresholdStatus.Draining:
                    case AccessBadgeThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case AccessBadgeThresholdStatus.Draining:
                switch (next)
                {
                    case AccessBadgeThresholdStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccessBadgeThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue + (double)Capacity, 2);
    }

    public AccessBadgeThreshold Copy()
    {
        return new AccessBadgeThreshold(
            Code,
            Name,
            Unit,
            Region,
            FloorValue,
            Badge,
            Capacity,
            Channel,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
