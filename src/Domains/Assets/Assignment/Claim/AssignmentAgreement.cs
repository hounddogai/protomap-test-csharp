namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment agreement is in its life cycle.</summary>
public enum AssignmentAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One assignment agreement, with what the planners need to act on it.</summary>
public sealed class AssignmentAgreement(string code, string title, double cost, string employeeId, string phone, double rating, double rate, bool active, string summary, AssignmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public string Summary { get; set; } = summary;
    public AssignmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a assignment agreement in status planned.</summary>
    public static AssignmentAgreement NewAssignmentAgreement(string code, string title)
    {
        return new AssignmentAgreement(
            code,
            title,
            0.0,
            "",
            "",
            0.0,
            0.0,
            false,
            "",
            AssignmentAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentAgreementStatus next)
    {
        switch (Status)
        {
            case AssignmentAgreementStatus.Planned:
                return next == AssignmentAgreementStatus.Scheduled || next == AssignmentAgreementStatus.InProgress;
            case AssignmentAgreementStatus.Scheduled:
                return next == AssignmentAgreementStatus.Cancelled;
            case AssignmentAgreementStatus.InProgress:
                return next == AssignmentAgreementStatus.Completed;
            case AssignmentAgreementStatus.Completed:
                switch (next)
                {
                    case AssignmentAgreementStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Rate * Rating, 2);
    }

    public AssignmentAgreement Copy()
    {
        return new AssignmentAgreement(Code, Title, Cost, EmployeeId, Phone, Rating, Rate, Active, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
