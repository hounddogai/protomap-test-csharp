namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding rollup is in its life cycle.</summary>
public enum OffboardingRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A offboarding rollup as the offboarding team keeps it.</summary>
public sealed class OffboardingRollup(string code, string label, string region, bool visible, double budget, int retries, int createdAt, int startsAt, OffboardingRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Region { get; set; } = region;
    public bool Visible { get; set; } = visible;
    public double Budget { get; set; } = budget;
    public int Retries { get; set; } = retries;
    public int CreatedAt { get; set; } = createdAt;
    public int StartsAt { get; set; } = startsAt;
    public OffboardingRollupStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding rollup in status idle.</summary>
    public static OffboardingRollup NewOffboardingRollup(string code, string label)
    {
        return new OffboardingRollup(code, label, "", false, 0.0, 0, 0, 0, OffboardingRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingRollupStatus next)
    {
        switch (Status)
        {
            case OffboardingRollupStatus.Idle:
                switch (next)
                {
                    case OffboardingRollupStatus.Draining:
                        return true;
                }
                return false;
            case OffboardingRollupStatus.Warming:
                return next == OffboardingRollupStatus.Ready || next == OffboardingRollupStatus.Stopped;
            case OffboardingRollupStatus.Ready:
                return next == OffboardingRollupStatus.Draining;
            case OffboardingRollupStatus.Draining:
                return next == OffboardingRollupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(OffboardingRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Retries + (double)CreatedAt, 2);
    }

    public OffboardingRollup Copy()
    {
        return new OffboardingRollup(Code, Label, Region, Visible, Budget, Retries, CreatedAt, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
