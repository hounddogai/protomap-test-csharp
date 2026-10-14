namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict warranty budget is in its life cycle.</summary>
public enum StrictWarrantyBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One strict warranty budget, with what the planners need to act on it.</summary>
public sealed class StrictWarrantyBudget(string code, string title, string reviewer, int revision, string source, StrictWarrantyBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int Revision { get; set; } = revision;
    public string Source { get; set; } = source;
    public StrictWarrantyBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a strict warranty budget in status new.</summary>
    public static StrictWarrantyBudget NewStrictWarrantyBudget(string code, string title)
    {
        return new StrictWarrantyBudget(code, title, "", 0, "", StrictWarrantyBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictWarrantyBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictWarrantyBudgetStatus next)
    {
        switch (Status)
        {
            case StrictWarrantyBudgetStatus.New:
                return next == StrictWarrantyBudgetStatus.Failed;
            case StrictWarrantyBudgetStatus.Queued:
                return next == StrictWarrantyBudgetStatus.Done || next == StrictWarrantyBudgetStatus.Failed;
            case StrictWarrantyBudgetStatus.Running:
                return next == StrictWarrantyBudgetStatus.Done;
            case StrictWarrantyBudgetStatus.Done:
                switch (next)
                {
                    case StrictWarrantyBudgetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictWarrantyBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict warranty budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"strict warranty budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Revision;
    }

    public StrictWarrantyBudget Copy()
    {
        return new StrictWarrantyBudget(Code, Title, Reviewer, Revision, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
