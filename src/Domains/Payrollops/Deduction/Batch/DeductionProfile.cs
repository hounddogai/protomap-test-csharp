namespace Acme.Workforce.Payrollops.Deduction.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction profile is in its life cycle.</summary>
public enum DeductionProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One deduction profile, with what the planners need to act on it.</summary>
public sealed class DeductionProfile(string code, string title, string employeeId, bool active, double cost, DeductionProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public DeductionProfileStatus Status { get; set; } = status;

    /// <summary>Starts a deduction profile in status open.</summary>
    public static DeductionProfile NewDeductionProfile(string code, string title)
    {
        return new DeductionProfile(code, title, "", false, 0.0, DeductionProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionProfileStatus next)
    {
        switch (Status)
        {
            case DeductionProfileStatus.Open:
                return next == DeductionProfileStatus.Reopened || next == DeductionProfileStatus.Archived;
            case DeductionProfileStatus.InReview:
                switch (next)
                {
                    case DeductionProfileStatus.Resolved:
                    case DeductionProfileStatus.Reopened:
                        return true;
                }
                return false;
            case DeductionProfileStatus.Resolved:
                return next == DeductionProfileStatus.Reopened;
            case DeductionProfileStatus.Reopened:
                switch (next)
                {
                    case DeductionProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeductionProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public DeductionProfile Copy()
    {
        return new DeductionProfile(Code, Title, EmployeeId, Active, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
