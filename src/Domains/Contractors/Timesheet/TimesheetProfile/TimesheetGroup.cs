namespace Acme.Workforce.Contractors.Timesheet.TimesheetProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet group is in its life cycle.</summary>
public enum TimesheetGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A timesheet group. Its code is unique within a tenant.</summary>
public sealed class TimesheetGroup(string code, string title, bool pinned, int retries, string employeeId, double score, double discount, string location, string checksum, TimesheetGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public int Retries { get; set; } = retries;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public double Discount { get; set; } = discount;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public TimesheetGroupStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet group in status requested.</summary>
    public static TimesheetGroup NewTimesheetGroup(string code, string title)
    {
        return new TimesheetGroup(code, title, false, 0, "", 0.0, 0.0, "", "", TimesheetGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetGroupStatus next)
    {
        switch (Status)
        {
            case TimesheetGroupStatus.Requested:
                switch (next)
                {
                    case TimesheetGroupStatus.Revoked:
                    case TimesheetGroupStatus.Expired:
                        return true;
                }
                return false;
            case TimesheetGroupStatus.Granted:
                switch (next)
                {
                    case TimesheetGroupStatus.Revoked:
                    case TimesheetGroupStatus.Expired:
                        return true;
                }
                return false;
            case TimesheetGroupStatus.Revoked:
                return next == TimesheetGroupStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TimesheetGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Discount - Score, 2);
    }

    public TimesheetGroup Copy()
    {
        return new TimesheetGroup(
            Code,
            Title,
            Pinned,
            Retries,
            EmployeeId,
            Score,
            Discount,
            Location,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
