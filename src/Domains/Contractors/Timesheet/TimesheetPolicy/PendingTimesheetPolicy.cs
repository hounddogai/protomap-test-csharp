namespace Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending timesheet policy is in its life cycle.</summary>
public enum PendingTimesheetPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pending timesheet policy, with what the planners need to act on it.</summary>
public sealed class PendingTimesheetPolicy(string code, string title, string category, string checksum, int durationMinutes, string unit, int graceDays, int leaveDays, string currency, PendingTimesheetPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Checksum { get; set; } = checksum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Unit { get; set; } = unit;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public string Currency { get; set; } = currency;
    public PendingTimesheetPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a pending timesheet policy in status open.</summary>
    public static PendingTimesheetPolicy NewPendingTimesheetPolicy(string code, string title)
    {
        return new PendingTimesheetPolicy(code, title, "", "", 0, "", 0, 0, "", PendingTimesheetPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTimesheetPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTimesheetPolicyStatus next)
    {
        switch (Status)
        {
            case PendingTimesheetPolicyStatus.Open:
                return next == PendingTimesheetPolicyStatus.InReview || next == PendingTimesheetPolicyStatus.Archived;
            case PendingTimesheetPolicyStatus.InReview:
                switch (next)
                {
                    case PendingTimesheetPolicyStatus.Archived:
                        return true;
                }
                return false;
            case PendingTimesheetPolicyStatus.Resolved:
                return next == PendingTimesheetPolicyStatus.Reopened;
            case PendingTimesheetPolicyStatus.Reopened:
                return next == PendingTimesheetPolicyStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PendingTimesheetPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending timesheet policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays + (double)GraceDays, 2);
    }

    public PendingTimesheetPolicy Copy()
    {
        return new PendingTimesheetPolicy(
            Code,
            Title,
            Category,
            Checksum,
            DurationMinutes,
            Unit,
            GraceDays,
            LeaveDays,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
