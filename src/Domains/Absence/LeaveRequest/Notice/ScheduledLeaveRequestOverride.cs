namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled leave request override is in its life cycle.</summary>
public enum ScheduledLeaveRequestOverrideStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A scheduled leave request override as the leave request team keeps it.</summary>
public sealed class ScheduledLeaveRequestOverride(string code, string name, string jobCode, string locale, double rating, string channel, double margin, ScheduledLeaveRequestOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Locale { get; set; } = locale;
    public double Rating { get; set; } = rating;
    public string Channel { get; set; } = channel;
    public double Margin { get; set; } = margin;
    public ScheduledLeaveRequestOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled leave request override in status proposed.</summary>
    public static ScheduledLeaveRequestOverride NewScheduledLeaveRequestOverride(string code, string name)
    {
        return new ScheduledLeaveRequestOverride(
            code,
            name,
            "",
            "",
            0.0,
            "",
            0.0,
            ScheduledLeaveRequestOverrideStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledLeaveRequestOverrideStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledLeaveRequestOverrideStatus next)
    {
        switch (Status)
        {
            case ScheduledLeaveRequestOverrideStatus.Proposed:
                return next == ScheduledLeaveRequestOverrideStatus.Rejected;
            case ScheduledLeaveRequestOverrideStatus.Approved:
                switch (next)
                {
                    case ScheduledLeaveRequestOverrideStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledLeaveRequestOverrideStatus.Rejected:
                return next == ScheduledLeaveRequestOverrideStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ScheduledLeaveRequestOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled leave request override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetVolume()
    {
        return Math.Round(Margin * Rating, 2);
    }

    public ScheduledLeaveRequestOverride Copy()
    {
        return new ScheduledLeaveRequestOverride(Code, Name, JobCode, Locale, Rating, Channel, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
