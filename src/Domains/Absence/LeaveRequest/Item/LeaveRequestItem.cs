namespace Acme.Workforce.Absence.LeaveRequest.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request item is in its life cycle.</summary>
public enum LeaveRequestItemStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a leave request item and its current status.</summary>
public sealed class LeaveRequestItem(string code, string title, int revision, string source, int version, int leaveDays, double ceiling, string description, bool visible, double ratio, int durationMinutes, LeaveRequestItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ceiling { get; set; } = ceiling;
    public string Description { get; set; } = description;
    public bool Visible { get; set; } = visible;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LeaveRequestItemStatus Status { get; set; } = status;

    /// <summary>Starts a leave request item in status created.</summary>
    public static LeaveRequestItem NewLeaveRequestItem(string code, string title)
    {
        return new LeaveRequestItem(
            code,
            title,
            0,
            "",
            0,
            0,
            0.0,
            "",
            false,
            0.0,
            0,
            LeaveRequestItemStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestItemStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestItemStatus next)
    {
        switch (Status)
        {
            case LeaveRequestItemStatus.Created:
                return next == LeaveRequestItemStatus.Validated || next == LeaveRequestItemStatus.Retired;
            case LeaveRequestItemStatus.Validated:
                return next == LeaveRequestItemStatus.Published;
            case LeaveRequestItemStatus.Published:
                switch (next)
                {
                    case LeaveRequestItemStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Ceiling + Ratio, 2);
    }

    public LeaveRequestItem Copy()
    {
        return new LeaveRequestItem(
            Code,
            Title,
            Revision,
            Source,
            Version,
            LeaveDays,
            Ceiling,
            Description,
            Visible,
            Ratio,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
