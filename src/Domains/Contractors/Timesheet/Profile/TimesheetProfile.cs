namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet profile is in its life cycle.</summary>
public enum TimesheetProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a timesheet profile and its current status.</summary>
public sealed class TimesheetProfile(string code, string name, string badge, string team, int expiresAt, bool pinned, int startsAt, string description, TimesheetProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public int StartsAt { get; set; } = startsAt;
    public string Description { get; set; } = description;
    public TimesheetProfileStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet profile in status proposed.</summary>
    public static TimesheetProfile NewTimesheetProfile(string code, string name)
    {
        return new TimesheetProfile(code, name, "", "", 0, false, 0, "", TimesheetProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetProfileStatus next)
    {
        switch (Status)
        {
            case TimesheetProfileStatus.Proposed:
                return next == TimesheetProfileStatus.Rejected;
            case TimesheetProfileStatus.Approved:
                return next == TimesheetProfileStatus.Rejected || next == TimesheetProfileStatus.Withdrawn;
            case TimesheetProfileStatus.Rejected:
                return next == TimesheetProfileStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TimesheetProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)StartsAt * (double)ExpiresAt, 2);
    }

    public TimesheetProfile Copy()
    {
        return new TimesheetProfile(Code, Name, Badge, Team, ExpiresAt, Pinned, StartsAt, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
