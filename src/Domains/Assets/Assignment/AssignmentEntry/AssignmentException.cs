namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment exception is in its life cycle.</summary>
public enum AssignmentExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One assignment exception, with what the planners need to act on it.</summary>
public sealed class AssignmentException(string code, string title, string salaryBand, string notes, double hours, double maximum, string location, int durationMinutes, AssignmentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public string Notes { get; set; } = notes;
    public double Hours { get; set; } = hours;
    public double Maximum { get; set; } = maximum;
    public string Location { get; set; } = location;
    public int DurationMinutes { get; set; } = durationMinutes;
    public AssignmentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a assignment exception in status proposed.</summary>
    public static AssignmentException NewAssignmentException(string code, string title)
    {
        return new AssignmentException(code, title, "", "", 0.0, 0.0, "", 0, AssignmentExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentExceptionStatus next)
    {
        switch (Status)
        {
            case AssignmentExceptionStatus.Proposed:
                return next == AssignmentExceptionStatus.Approved || next == AssignmentExceptionStatus.Withdrawn;
            case AssignmentExceptionStatus.Approved:
                return next == AssignmentExceptionStatus.Withdrawn;
            case AssignmentExceptionStatus.Rejected:
                switch (next)
                {
                    case AssignmentExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Hours + Maximum, 2);
    }

    public AssignmentException Copy()
    {
        return new AssignmentException(
            Code,
            Title,
            SalaryBand,
            Notes,
            Hours,
            Maximum,
            Location,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
