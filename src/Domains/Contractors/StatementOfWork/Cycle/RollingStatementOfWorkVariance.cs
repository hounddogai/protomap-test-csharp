namespace Acme.Workforce.Contractors.StatementOfWork.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling statement of work variance is in its life cycle.</summary>
public enum RollingStatementOfWorkVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling statement of work variance as the statement of work team keeps it.</summary>
public sealed class RollingStatementOfWorkVariance(string code, string title, bool pinned, string grade, int capacity, string description, int priority, string department, RollingStatementOfWorkVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Grade { get; set; } = grade;
    public int Capacity { get; set; } = capacity;
    public string Description { get; set; } = description;
    public int Priority { get; set; } = priority;
    public string Department { get; set; } = department;
    public RollingStatementOfWorkVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling statement of work variance in status created.</summary>
    public static RollingStatementOfWorkVariance NewRollingStatementOfWorkVariance(string code, string title)
    {
        return new RollingStatementOfWorkVariance(
            code,
            title,
            false,
            "",
            0,
            "",
            0,
            "",
            RollingStatementOfWorkVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingStatementOfWorkVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingStatementOfWorkVarianceStatus next)
    {
        switch (Status)
        {
            case RollingStatementOfWorkVarianceStatus.Created:
                return next == RollingStatementOfWorkVarianceStatus.Retired;
            case RollingStatementOfWorkVarianceStatus.Validated:
                switch (next)
                {
                    case RollingStatementOfWorkVarianceStatus.Published:
                    case RollingStatementOfWorkVarianceStatus.Retired:
                        return true;
                }
                return false;
            case RollingStatementOfWorkVarianceStatus.Published:
                switch (next)
                {
                    case RollingStatementOfWorkVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingStatementOfWorkVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling statement of work variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Priority - (double)Capacity, 2);
    }

    public RollingStatementOfWorkVariance Copy()
    {
        return new RollingStatementOfWorkVariance(
            Code,
            Title,
            Pinned,
            Grade,
            Capacity,
            Description,
            Priority,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
