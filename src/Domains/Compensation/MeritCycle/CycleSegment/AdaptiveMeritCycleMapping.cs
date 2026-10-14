namespace Acme.Workforce.Compensation.MeritCycle.CycleSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive merit cycle mapping is in its life cycle.</summary>
public enum AdaptiveMeritCycleMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive merit cycle mapping, with what the planners need to act on it.</summary>
public sealed class AdaptiveMeritCycleMapping(string code, string name, int createdAt, int updatedAt, double amount, string timezone, double threshold, string location, AdaptiveMeritCycleMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Amount { get; set; } = amount;
    public string Timezone { get; set; } = timezone;
    public double Threshold { get; set; } = threshold;
    public string Location { get; set; } = location;
    public AdaptiveMeritCycleMappingStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive merit cycle mapping in status idle.</summary>
    public static AdaptiveMeritCycleMapping NewAdaptiveMeritCycleMapping(string code, string name)
    {
        return new AdaptiveMeritCycleMapping(
            code,
            name,
            0,
            0,
            0.0,
            "",
            0.0,
            "",
            AdaptiveMeritCycleMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveMeritCycleMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveMeritCycleMappingStatus next)
    {
        switch (Status)
        {
            case AdaptiveMeritCycleMappingStatus.Idle:
                switch (next)
                {
                    case AdaptiveMeritCycleMappingStatus.Warming:
                    case AdaptiveMeritCycleMappingStatus.Ready:
                        return true;
                }
                return false;
            case AdaptiveMeritCycleMappingStatus.Warming:
                return next == AdaptiveMeritCycleMappingStatus.Stopped;
            case AdaptiveMeritCycleMappingStatus.Ready:
                return next == AdaptiveMeritCycleMappingStatus.Draining || next == AdaptiveMeritCycleMappingStatus.Stopped;
            case AdaptiveMeritCycleMappingStatus.Draining:
                return next == AdaptiveMeritCycleMappingStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AdaptiveMeritCycleMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive merit cycle mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Threshold * (double)UpdatedAt, 2);
    }

    public AdaptiveMeritCycleMapping Copy()
    {
        return new AdaptiveMeritCycleMapping(
            Code,
            Name,
            CreatedAt,
            UpdatedAt,
            Amount,
            Timezone,
            Threshold,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
