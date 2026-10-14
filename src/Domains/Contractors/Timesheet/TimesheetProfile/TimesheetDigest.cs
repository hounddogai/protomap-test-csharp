namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet digest is in its life cycle.</summary>
public enum TimesheetDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A timesheet digest. Its code is unique within a tenant.</summary>
public sealed class TimesheetDigest(string code, string label, string category, string segment, int tenureMonths, double budget, string summary, int attempts, TimesheetDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public TimesheetDigestStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet digest in status requested.</summary>
    public static TimesheetDigest NewTimesheetDigest(string code, string label)
    {
        return new TimesheetDigest(code, label, "", "", 0, 0.0, "", 0, TimesheetDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetDigestStatus next)
    {
        switch (Status)
        {
            case TimesheetDigestStatus.Requested:
                return next == TimesheetDigestStatus.Expired;
            case TimesheetDigestStatus.Granted:
                switch (next)
                {
                    case TimesheetDigestStatus.Revoked:
                    case TimesheetDigestStatus.Expired:
                        return true;
                }
                return false;
            case TimesheetDigestStatus.Revoked:
                return next == TimesheetDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TimesheetDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Budget + (double)Attempts, 2);
    }

    public TimesheetDigest Copy()
    {
        return new TimesheetDigest(Code, Label, Category, Segment, TenureMonths, Budget, Summary, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
