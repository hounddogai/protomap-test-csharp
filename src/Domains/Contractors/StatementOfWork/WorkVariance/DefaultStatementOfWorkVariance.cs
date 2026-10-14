namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default statement of work variance is in its life cycle.</summary>
public enum DefaultStatementOfWorkVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default statement of work variance. Its code is unique within a tenant.</summary>
public sealed class DefaultStatementOfWorkVariance(string code, string name, int version, string jobCode, double budget, DefaultStatementOfWorkVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string JobCode { get; set; } = jobCode;
    public double Budget { get; set; } = budget;
    public DefaultStatementOfWorkVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a default statement of work variance in status proposed.</summary>
    public static DefaultStatementOfWorkVariance NewDefaultStatementOfWorkVariance(string code, string name)
    {
        return new DefaultStatementOfWorkVariance(
            code,
            name,
            0,
            "",
            0.0,
            DefaultStatementOfWorkVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultStatementOfWorkVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultStatementOfWorkVarianceStatus next)
    {
        switch (Status)
        {
            case DefaultStatementOfWorkVarianceStatus.Proposed:
                switch (next)
                {
                    case DefaultStatementOfWorkVarianceStatus.Approved:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkVarianceStatus.Approved:
                switch (next)
                {
                    case DefaultStatementOfWorkVarianceStatus.Rejected:
                    case DefaultStatementOfWorkVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkVarianceStatus.Rejected:
                switch (next)
                {
                    case DefaultStatementOfWorkVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultStatementOfWorkVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default statement of work variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"default statement of work variance"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Version + Budget, 2);
    }

    public DefaultStatementOfWorkVariance Copy()
    {
        return new DefaultStatementOfWorkVariance(Code, Name, Version, JobCode, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
