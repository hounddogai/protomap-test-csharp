namespace Acme.Workforce.Workplace.AccessBadge.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge budget is in its life cycle.</summary>
public enum AccessBadgeBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A access badge budget. Its code is unique within a tenant.</summary>
public sealed class AccessBadgeBudget(string code, string name, string notes, int revision, int leaveDays, AccessBadgeBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public int Revision { get; set; } = revision;
    public int LeaveDays { get; set; } = leaveDays;
    public AccessBadgeBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a access badge budget in status open.</summary>
    public static AccessBadgeBudget NewAccessBadgeBudget(string code, string name)
    {
        return new AccessBadgeBudget(code, name, "", 0, 0, AccessBadgeBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeBudgetStatus next)
    {
        switch (Status)
        {
            case AccessBadgeBudgetStatus.Open:
                return next == AccessBadgeBudgetStatus.InReview || next == AccessBadgeBudgetStatus.Archived;
            case AccessBadgeBudgetStatus.InReview:
                return next == AccessBadgeBudgetStatus.Resolved || next == AccessBadgeBudgetStatus.Reopened;
            case AccessBadgeBudgetStatus.Resolved:
                return next == AccessBadgeBudgetStatus.Reopened || next == AccessBadgeBudgetStatus.Archived;
            case AccessBadgeBudgetStatus.Reopened:
                return next == AccessBadgeBudgetStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AccessBadgeBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"access badge budget"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays - (double)Revision, 2);
    }

    public AccessBadgeBudget Copy()
    {
        return new AccessBadgeBudget(Code, Name, Notes, Revision, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
