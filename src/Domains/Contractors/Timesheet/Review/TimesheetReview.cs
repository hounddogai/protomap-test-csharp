namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet review is in its life cycle.</summary>
public enum TimesheetReviewStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One timesheet review, with what the planners need to act on it.</summary>
public sealed class TimesheetReview(string code, string label, string locale, string employeeId, bool approved, double minimum, string jobCode, int expiresAt, double ratio, int quantity, double margin, TimesheetReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string EmployeeId { get; set; } = employeeId;
    public bool Approved { get; set; } = approved;
    public double Minimum { get; set; } = minimum;
    public string JobCode { get; set; } = jobCode;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Ratio { get; set; } = ratio;
    public int Quantity { get; set; } = quantity;
    public double Margin { get; set; } = margin;
    public TimesheetReviewStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet review in status created.</summary>
    public static TimesheetReview NewTimesheetReview(string code, string label)
    {
        return new TimesheetReview(
            code,
            label,
            "",
            "",
            false,
            0.0,
            "",
            0,
            0.0,
            0,
            0.0,
            TimesheetReviewStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetReviewStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetReviewStatus next)
    {
        switch (Status)
        {
            case TimesheetReviewStatus.Created:
                return next == TimesheetReviewStatus.Validated;
            case TimesheetReviewStatus.Validated:
                return next == TimesheetReviewStatus.Retired;
            case TimesheetReviewStatus.Published:
                return next == TimesheetReviewStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TimesheetReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"timesheet review"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ratio * (double)ExpiresAt, 2);
    }

    public TimesheetReview Copy()
    {
        return new TimesheetReview(
            Code,
            Label,
            Locale,
            EmployeeId,
            Approved,
            Minimum,
            JobCode,
            ExpiresAt,
            Ratio,
            Quantity,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
