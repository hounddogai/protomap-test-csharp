namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work exception is in its life cycle.</summary>
public enum StatementOfWorkExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One statement of work exception, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkException(string code, string name, int expiresAt, string summary, string category, StatementOfWorkExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public StatementOfWorkExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work exception in status proposed.</summary>
    public static StatementOfWorkException NewStatementOfWorkException(string code, string name)
    {
        return new StatementOfWorkException(code, name, 0, "", "", StatementOfWorkExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkExceptionStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkExceptionStatus.Proposed:
                switch (next)
                {
                    case StatementOfWorkExceptionStatus.Rejected:
                        return true;
                }
                return false;
            case StatementOfWorkExceptionStatus.Approved:
                return next == StatementOfWorkExceptionStatus.Rejected;
            case StatementOfWorkExceptionStatus.Rejected:
                return next == StatementOfWorkExceptionStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public StatementOfWorkException Copy()
    {
        return new StatementOfWorkException(Code, Name, ExpiresAt, Summary, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
