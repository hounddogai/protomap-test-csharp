namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled headcount budget is in its life cycle.</summary>
public enum ScheduledHeadcountBudgetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A scheduled headcount budget. Its code is unique within a tenant.</summary>
public sealed class ScheduledHeadcountBudget(string code, string name, int revision, string badge, string locale, ScheduledHeadcountBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Badge { get; set; } = badge;
    public string Locale { get; set; } = locale;
    public ScheduledHeadcountBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled headcount budget in status planned.</summary>
    public static ScheduledHeadcountBudget NewScheduledHeadcountBudget(string code, string name)
    {
        return new ScheduledHeadcountBudget(code, name, 0, "", "", ScheduledHeadcountBudgetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledHeadcountBudgetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledHeadcountBudgetStatus next)
    {
        switch (Status)
        {
            case ScheduledHeadcountBudgetStatus.Planned:
                return next == ScheduledHeadcountBudgetStatus.Scheduled || next == ScheduledHeadcountBudgetStatus.InProgress;
            case ScheduledHeadcountBudgetStatus.Scheduled:
                switch (next)
                {
                    case ScheduledHeadcountBudgetStatus.Cancelled:
                        return true;
                }
                return false;
            case ScheduledHeadcountBudgetStatus.InProgress:
                return next == ScheduledHeadcountBudgetStatus.Completed || next == ScheduledHeadcountBudgetStatus.Cancelled;
            case ScheduledHeadcountBudgetStatus.Completed:
                switch (next)
                {
                    case ScheduledHeadcountBudgetStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledHeadcountBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled headcount budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"scheduled headcount budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public ScheduledHeadcountBudget Copy()
    {
        return new ScheduledHeadcountBudget(Code, Name, Revision, Badge, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
