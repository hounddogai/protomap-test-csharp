namespace Acme.Workforce.Contractors.Timesheet.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet agreement is in its life cycle.</summary>
public enum TimesheetAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One timesheet agreement, with what the planners need to act on it.</summary>
public sealed class TimesheetAgreement(string code, string label, int capacity, double margin, int priority, string locale, int endsAt, double hours, string category, int retries, int tenureMonths, TimesheetAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Margin { get; set; } = margin;
    public int Priority { get; set; } = priority;
    public string Locale { get; set; } = locale;
    public int EndsAt { get; set; } = endsAt;
    public double Hours { get; set; } = hours;
    public string Category { get; set; } = category;
    public int Retries { get; set; } = retries;
    public int TenureMonths { get; set; } = tenureMonths;
    public TimesheetAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet agreement in status open.</summary>
    public static TimesheetAgreement NewTimesheetAgreement(string code, string label)
    {
        return new TimesheetAgreement(code, label, 0, 0.0, 0, "", 0, 0.0, "", 0, 0, TimesheetAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetAgreementStatus next)
    {
        switch (Status)
        {
            case TimesheetAgreementStatus.Open:
                return next == TimesheetAgreementStatus.Resolved || next == TimesheetAgreementStatus.Reopened;
            case TimesheetAgreementStatus.InReview:
                switch (next)
                {
                    case TimesheetAgreementStatus.Resolved:
                    case TimesheetAgreementStatus.Reopened:
                        return true;
                }
                return false;
            case TimesheetAgreementStatus.Resolved:
                return next == TimesheetAgreementStatus.Reopened;
            case TimesheetAgreementStatus.Reopened:
                return next == TimesheetAgreementStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TimesheetAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Capacity + Margin, 2);
    }

    public TimesheetAgreement Copy()
    {
        return new TimesheetAgreement(
            Code,
            Label,
            Capacity,
            Margin,
            Priority,
            Locale,
            EndsAt,
            Hours,
            Category,
            Retries,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
