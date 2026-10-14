namespace Acme.Workforce.Talent.Nomination.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default nomination assignment is in its life cycle.</summary>
public enum DefaultNominationAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a default nomination assignment and its current status.</summary>
public sealed class DefaultNominationAssignment(string code, string title, string phone, string description, string salaryBand, double minimum, int attempts, string externalRef, DefaultNominationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public string Description { get; set; } = description;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public int Attempts { get; set; } = attempts;
    public string ExternalRef { get; set; } = externalRef;
    public DefaultNominationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a default nomination assignment in status created.</summary>
    public static DefaultNominationAssignment NewDefaultNominationAssignment(string code, string title)
    {
        return new DefaultNominationAssignment(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0,
            "",
            DefaultNominationAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultNominationAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultNominationAssignmentStatus next)
    {
        switch (Status)
        {
            case DefaultNominationAssignmentStatus.Created:
                return next == DefaultNominationAssignmentStatus.Validated || next == DefaultNominationAssignmentStatus.Published;
            case DefaultNominationAssignmentStatus.Validated:
                switch (next)
                {
                    case DefaultNominationAssignmentStatus.Published:
                        return true;
                }
                return false;
            case DefaultNominationAssignmentStatus.Published:
                return next == DefaultNominationAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DefaultNominationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default nomination assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round(Minimum * (double)Attempts, 2);
    }

    public DefaultNominationAssignment Copy()
    {
        return new DefaultNominationAssignment(
            Code,
            Title,
            Phone,
            Description,
            SalaryBand,
            Minimum,
            Attempts,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
