namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling merit cycle target is in its life cycle.</summary>
public enum RollingMeritCycleTargetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling merit cycle target. Its code is unique within a tenant.</summary>
public sealed class RollingMeritCycleTarget(string code, string title, int retries, bool approved, int expiresAt, int leaveDays, double discount, string grade, int endsAt, int priority, RollingMeritCycleTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public bool Approved { get; set; } = approved;
    public int ExpiresAt { get; set; } = expiresAt;
    public int LeaveDays { get; set; } = leaveDays;
    public double Discount { get; set; } = discount;
    public string Grade { get; set; } = grade;
    public int EndsAt { get; set; } = endsAt;
    public int Priority { get; set; } = priority;
    public RollingMeritCycleTargetStatus Status { get; set; } = status;

    /// <summary>Starts a rolling merit cycle target in status idle.</summary>
    public static RollingMeritCycleTarget NewRollingMeritCycleTarget(string code, string title)
    {
        return new RollingMeritCycleTarget(
            code,
            title,
            0,
            false,
            0,
            0,
            0.0,
            "",
            0,
            0,
            RollingMeritCycleTargetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMeritCycleTargetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMeritCycleTargetStatus next)
    {
        switch (Status)
        {
            case RollingMeritCycleTargetStatus.Idle:
                switch (next)
                {
                    case RollingMeritCycleTargetStatus.Ready:
                        return true;
                }
                return false;
            case RollingMeritCycleTargetStatus.Warming:
                switch (next)
                {
                    case RollingMeritCycleTargetStatus.Ready:
                    case RollingMeritCycleTargetStatus.Draining:
                        return true;
                }
                return false;
            case RollingMeritCycleTargetStatus.Ready:
                return next == RollingMeritCycleTargetStatus.Stopped;
            case RollingMeritCycleTargetStatus.Draining:
                return next == RollingMeritCycleTargetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingMeritCycleTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling merit cycle target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries + (double)LeaveDays, 2);
    }

    public RollingMeritCycleTarget Copy()
    {
        return new RollingMeritCycleTarget(
            Code,
            Title,
            Retries,
            Approved,
            ExpiresAt,
            LeaveDays,
            Discount,
            Grade,
            EndsAt,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
