namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work target is in its life cycle.</summary>
public enum StatementOfWorkTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One statement of work target, with what the planners need to act on it.</summary>
public sealed class StatementOfWorkTarget(string code, string title, string category, int startsAt, string name, double fte, StatementOfWorkTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public StatementOfWorkTargetStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work target in status planned.</summary>
    public static StatementOfWorkTarget NewStatementOfWorkTarget(string code, string title)
    {
        return new StatementOfWorkTarget(code, title, "", 0, "", 0.0, StatementOfWorkTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkTargetStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkTargetStatus.Planned:
                return next == StatementOfWorkTargetStatus.Completed;
            case StatementOfWorkTargetStatus.Scheduled:
                switch (next)
                {
                    case StatementOfWorkTargetStatus.InProgress:
                    case StatementOfWorkTargetStatus.Completed:
                        return true;
                }
                return false;
            case StatementOfWorkTargetStatus.InProgress:
                return next == StatementOfWorkTargetStatus.Cancelled;
            case StatementOfWorkTargetStatus.Completed:
                switch (next)
                {
                    case StatementOfWorkTargetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedValue()
    {
        return Math.Round(Fte - (double)StartsAt, 2);
    }

    public StatementOfWorkTarget Copy()
    {
        return new StatementOfWorkTarget(Code, Title, Category, StartsAt, Name, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
