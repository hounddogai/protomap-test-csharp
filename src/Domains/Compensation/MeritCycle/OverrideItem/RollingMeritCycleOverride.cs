namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling merit cycle override is in its life cycle.</summary>
public enum RollingMeritCycleOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling merit cycle override. Its code is unique within a tenant.</summary>
public sealed class RollingMeritCycleOverride(string code, string name, string reviewer, string phone, string grade, int createdAt, double ceiling, double cost, double minimum, int startsAt, RollingMeritCycleOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Reviewer { get; set; } = reviewer;
    public string Phone { get; set; } = phone;
    public string Grade { get; set; } = grade;
    public int CreatedAt { get; set; } = createdAt;
    public double Ceiling { get; set; } = ceiling;
    public double Cost { get; set; } = cost;
    public double Minimum { get; set; } = minimum;
    public int StartsAt { get; set; } = startsAt;
    public RollingMeritCycleOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a rolling merit cycle override in status idle.</summary>
    public static RollingMeritCycleOverride NewRollingMeritCycleOverride(string code, string name)
    {
        return new RollingMeritCycleOverride(
            code,
            name,
            "",
            "",
            "",
            0,
            0.0,
            0.0,
            0.0,
            0,
            RollingMeritCycleOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMeritCycleOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMeritCycleOverrideStatus next)
    {
        switch (Status)
        {
            case RollingMeritCycleOverrideStatus.Idle:
                switch (next)
                {
                    case RollingMeritCycleOverrideStatus.Draining:
                    case RollingMeritCycleOverrideStatus.Stopped:
                        return true;
                }
                return false;
            case RollingMeritCycleOverrideStatus.Warming:
                switch (next)
                {
                    case RollingMeritCycleOverrideStatus.Draining:
                    case RollingMeritCycleOverrideStatus.Stopped:
                        return true;
                }
                return false;
            case RollingMeritCycleOverrideStatus.Ready:
                return next == RollingMeritCycleOverrideStatus.Draining || next == RollingMeritCycleOverrideStatus.Stopped;
            case RollingMeritCycleOverrideStatus.Draining:
                return next == RollingMeritCycleOverrideStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingMeritCycleOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling merit cycle override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Minimum - (double)CreatedAt, 2);
    }

    public RollingMeritCycleOverride Copy()
    {
        return new RollingMeritCycleOverride(
            Code,
            Name,
            Reviewer,
            Phone,
            Grade,
            CreatedAt,
            Ceiling,
            Cost,
            Minimum,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
