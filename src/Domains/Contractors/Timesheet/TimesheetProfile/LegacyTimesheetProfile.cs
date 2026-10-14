namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy timesheet profile is in its life cycle.</summary>
public enum LegacyTimesheetProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy timesheet profile. Its code is unique within a tenant.</summary>
public sealed class LegacyTimesheetProfile(string code, string title, string category, int version, int revision, LegacyTimesheetProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int Version { get; set; } = version;
    public int Revision { get; set; } = revision;
    public LegacyTimesheetProfileStatus Status { get; set; } = status;

    /// <summary>Starts a legacy timesheet profile in status proposed.</summary>
    public static LegacyTimesheetProfile NewLegacyTimesheetProfile(string code, string title)
    {
        return new LegacyTimesheetProfile(code, title, "", 0, 0, LegacyTimesheetProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTimesheetProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTimesheetProfileStatus next)
    {
        switch (Status)
        {
            case LegacyTimesheetProfileStatus.Proposed:
                return next == LegacyTimesheetProfileStatus.Withdrawn;
            case LegacyTimesheetProfileStatus.Approved:
                return next == LegacyTimesheetProfileStatus.Rejected || next == LegacyTimesheetProfileStatus.Withdrawn;
            case LegacyTimesheetProfileStatus.Rejected:
                return next == LegacyTimesheetProfileStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyTimesheetProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy timesheet profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Revision - (double)Version, 2);
    }

    public LegacyTimesheetProfile Copy()
    {
        return new LegacyTimesheetProfile(Code, Title, Category, Version, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
