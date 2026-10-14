namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request baseline is in its life cycle.</summary>
public enum ScheduledLeaveRequestBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One scheduled leave request baseline, with what the planners need to act on it.</summary>
public sealed class ScheduledLeaveRequestBaseline(string code, string title, string grade, double ceiling, string region, double fte, string location, int updatedAt, int version, Dictionary<string, string> attributes, double ratio, ScheduledLeaveRequestBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public string Region { get; set; } = region;
    public double Fte { get; set; } = fte;
    public string Location { get; set; } = location;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ratio { get; set; } = ratio;
    public ScheduledLeaveRequestBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request baseline in status planned.</summary>
    public static ScheduledLeaveRequestBaseline NewScheduledLeaveRequestBaseline(string code, string title)
    {
        return new ScheduledLeaveRequestBaseline(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            "",
            0,
            0,
            new Dictionary<string, string>(),
            0.0,
            ScheduledLeaveRequestBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestBaselineStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestBaselineStatus.Planned:
                return next == ScheduledLeaveRequestBaselineStatus.InProgress || next == ScheduledLeaveRequestBaselineStatus.Completed;
            case ScheduledLeaveRequestBaselineStatus.Scheduled:
                return next == ScheduledLeaveRequestBaselineStatus.InProgress;
            case ScheduledLeaveRequestBaselineStatus.InProgress:
                switch (next)
                {
                    case ScheduledLeaveRequestBaselineStatus.Completed:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestBaselineStatus.Completed:
                return next == ScheduledLeaveRequestBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedExposure()
    {
        return Math.Round(Ceiling + Fte, 2);
    }

    public ScheduledLeaveRequestBaseline Copy()
    {
        return new ScheduledLeaveRequestBaseline(
            Code,
            Title,
            Grade,
            Ceiling,
            Region,
            Fte,
            Location,
            UpdatedAt,
            Version,
            Attributes,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
