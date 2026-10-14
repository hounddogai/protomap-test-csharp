namespace Acme.Workforce.Compliance.Exception.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a exception assignment is in its life cycle.</summary>
public enum ExceptionAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A exception assignment. Its code is unique within a tenant.</summary>
public sealed class ExceptionAssignment(string code, string title, bool archived, string locale, string country, string costCenter, double rate, string team, string summary, string location, ExceptionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public string Locale { get; set; } = locale;
    public string Country { get; set; } = country;
    public string CostCenter { get; set; } = costCenter;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public ExceptionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a exception assignment in status open.</summary>
    public static ExceptionAssignment NewExceptionAssignment(string code, string title)
    {
        return new ExceptionAssignment(
            code,
            title,
            false,
            "",
            "",
            "",
            0.0,
            "",
            "",
            "",
            ExceptionAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ExceptionAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ExceptionAssignmentStatus next)
    {
        switch (Status)
        {
            case ExceptionAssignmentStatus.Open:
                return next == ExceptionAssignmentStatus.InReview || next == ExceptionAssignmentStatus.Archived;
            case ExceptionAssignmentStatus.InReview:
                switch (next)
                {
                    case ExceptionAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case ExceptionAssignmentStatus.Resolved:
                return next == ExceptionAssignmentStatus.Reopened || next == ExceptionAssignmentStatus.Archived;
            case ExceptionAssignmentStatus.Reopened:
                return next == ExceptionAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ExceptionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"exception assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public ExceptionAssignment Copy()
    {
        return new ExceptionAssignment(
            Code,
            Title,
            Archived,
            Locale,
            Country,
            CostCenter,
            Rate,
            Team,
            Summary,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
