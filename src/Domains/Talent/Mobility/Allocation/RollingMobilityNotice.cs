namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling mobility notice is in its life cycle.</summary>
public enum RollingMobilityNoticeStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One rolling mobility notice, with what the planners need to act on it.</summary>
public sealed class RollingMobilityNotice(string code, string name, int tenureMonths, string summary, int expiresAt, RollingMobilityNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public int ExpiresAt { get; set; } = expiresAt;
    public RollingMobilityNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a rolling mobility notice in status planned.</summary>
    public static RollingMobilityNotice NewRollingMobilityNotice(string code, string name)
    {
        return new RollingMobilityNotice(code, name, 0, "", 0, RollingMobilityNoticeStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMobilityNoticeStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMobilityNoticeStatus next)
    {
        switch (Status)
        {
            case RollingMobilityNoticeStatus.Planned:
                switch (next)
                {
                    case RollingMobilityNoticeStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingMobilityNoticeStatus.Scheduled:
                return next == RollingMobilityNoticeStatus.InProgress || next == RollingMobilityNoticeStatus.Cancelled;
            case RollingMobilityNoticeStatus.InProgress:
                return next == RollingMobilityNoticeStatus.Completed || next == RollingMobilityNoticeStatus.Cancelled;
            case RollingMobilityNoticeStatus.Completed:
                return next == RollingMobilityNoticeStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RollingMobilityNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling mobility notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)TenureMonths + (double)ExpiresAt, 2);
    }

    public RollingMobilityNotice Copy()
    {
        return new RollingMobilityNotice(Code, Name, TenureMonths, Summary, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
