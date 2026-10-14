namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending headcount budget is in its life cycle.</summary>
public enum PendingHeadcountBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pending headcount budget and its current status.</summary>
public sealed class PendingHeadcountBudget(string code, string title, int windowDays, double rate, double cost, string jobCode, string reviewer, PendingHeadcountBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public double Rate { get; set; } = rate;
    public double Cost { get; set; } = cost;
    public string JobCode { get; set; } = jobCode;
    public string Reviewer { get; set; } = reviewer;
    public PendingHeadcountBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a pending headcount budget in status open.</summary>
    public static PendingHeadcountBudget NewPendingHeadcountBudget(string code, string title)
    {
        return new PendingHeadcountBudget(code, title, 0, 0.0, 0.0, "", "", PendingHeadcountBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingHeadcountBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingHeadcountBudgetStatus next)
    {
        switch (Status)
        {
            case PendingHeadcountBudgetStatus.Open:
                return next == PendingHeadcountBudgetStatus.Resolved;
            case PendingHeadcountBudgetStatus.InReview:
                return next == PendingHeadcountBudgetStatus.Resolved || next == PendingHeadcountBudgetStatus.Archived;
            case PendingHeadcountBudgetStatus.Resolved:
                switch (next)
                {
                    case PendingHeadcountBudgetStatus.Reopened:
                    case PendingHeadcountBudgetStatus.Archived:
                        return true;
                }
                return false;
            case PendingHeadcountBudgetStatus.Reopened:
                return next == PendingHeadcountBudgetStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PendingHeadcountBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending headcount budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending headcount budget"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Rate + Cost, 2);
    }

    public PendingHeadcountBudget Copy()
    {
        return new PendingHeadcountBudget(Code, Title, WindowDays, Rate, Cost, JobCode, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
