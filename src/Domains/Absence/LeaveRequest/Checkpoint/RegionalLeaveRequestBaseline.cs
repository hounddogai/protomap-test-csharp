namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional leave request baseline is in its life cycle.</summary>
public enum RegionalLeaveRequestBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional leave request baseline as the leave request team keeps it.</summary>
public sealed class RegionalLeaveRequestBaseline(string code, string name, int priority, double score, bool pinned, string summary, string segment, int startsAt, RegionalLeaveRequestBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public string Summary { get; set; } = summary;
    public string Segment { get; set; } = segment;
    public int StartsAt { get; set; } = startsAt;
    public RegionalLeaveRequestBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a regional leave request baseline in status idle.</summary>
    public static RegionalLeaveRequestBaseline NewRegionalLeaveRequestBaseline(string code, string name)
    {
        return new RegionalLeaveRequestBaseline(
            code,
            name,
            0,
            0.0,
            false,
            "",
            "",
            0,
            RegionalLeaveRequestBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalLeaveRequestBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalLeaveRequestBaselineStatus next)
    {
        switch (Status)
        {
            case RegionalLeaveRequestBaselineStatus.Idle:
                return next == RegionalLeaveRequestBaselineStatus.Warming || next == RegionalLeaveRequestBaselineStatus.Ready;
            case RegionalLeaveRequestBaselineStatus.Warming:
                switch (next)
                {
                    case RegionalLeaveRequestBaselineStatus.Draining:
                    case RegionalLeaveRequestBaselineStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalLeaveRequestBaselineStatus.Ready:
                return next == RegionalLeaveRequestBaselineStatus.Draining;
            case RegionalLeaveRequestBaselineStatus.Draining:
                switch (next)
                {
                    case RegionalLeaveRequestBaselineStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalLeaveRequestBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional leave request baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Score - (double)StartsAt, 2);
    }

    public RegionalLeaveRequestBaseline Copy()
    {
        return new RegionalLeaveRequestBaseline(
            Code,
            Name,
            Priority,
            Score,
            Pinned,
            Summary,
            Segment,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
