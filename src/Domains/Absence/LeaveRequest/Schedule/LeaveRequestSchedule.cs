namespace Acme.Workforce.Absence.LeaveRequest.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request schedule is in its life cycle.</summary>
public enum LeaveRequestScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A leave request schedule as the leave request team keeps it.</summary>
public sealed class LeaveRequestSchedule(string code, string title, string region, string checksum, string ownerId, double fte, int revision, string location, double minimum, LeaveRequestScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Checksum { get; set; } = checksum;
    public string OwnerId { get; set; } = ownerId;
    public double Fte { get; set; } = fte;
    public int Revision { get; set; } = revision;
    public string Location { get; set; } = location;
    public double Minimum { get; set; } = minimum;
    public LeaveRequestScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a leave request schedule in status idle.</summary>
    public static LeaveRequestSchedule NewLeaveRequestSchedule(string code, string title)
    {
        return new LeaveRequestSchedule(code, title, "", "", "", 0.0, 0, "", 0.0, LeaveRequestScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestScheduleStatus next)
    {
        switch (Status)
        {
            case LeaveRequestScheduleStatus.Idle:
                return next == LeaveRequestScheduleStatus.Warming || next == LeaveRequestScheduleStatus.Ready;
            case LeaveRequestScheduleStatus.Warming:
                return next == LeaveRequestScheduleStatus.Draining;
            case LeaveRequestScheduleStatus.Ready:
                switch (next)
                {
                    case LeaveRequestScheduleStatus.Draining:
                    case LeaveRequestScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case LeaveRequestScheduleStatus.Draining:
                switch (next)
                {
                    case LeaveRequestScheduleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"leave request schedule"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + (double)Revision, 2);
    }

    public LeaveRequestSchedule Copy()
    {
        return new LeaveRequestSchedule(
            Code,
            Title,
            Region,
            Checksum,
            OwnerId,
            Fte,
            Revision,
            Location,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
