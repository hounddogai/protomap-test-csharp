namespace Acme.Workforce.Assets.Warranty.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty budget is in its life cycle.</summary>
public enum WarrantyBudgetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a warranty budget and its current status.</summary>
public sealed class WarrantyBudget(string code, string name, bool visible, int leaveDays, string reviewer, int createdAt, double ceiling, string source, int windowDays, string category, int priority, WarrantyBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public string Reviewer { get; set; } = reviewer;
    public int CreatedAt { get; set; } = createdAt;
    public double Ceiling { get; set; } = ceiling;
    public string Source { get; set; } = source;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public int Priority { get; set; } = priority;
    public WarrantyBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a warranty budget in status new.</summary>
    public static WarrantyBudget NewWarrantyBudget(string code, string name)
    {
        return new WarrantyBudget(code, name, false, 0, "", 0, 0.0, "", 0, "", 0, WarrantyBudgetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyBudgetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyBudgetStatus next)
    {
        switch (Status)
        {
            case WarrantyBudgetStatus.New:
                return next == WarrantyBudgetStatus.Queued || next == WarrantyBudgetStatus.Failed;
            case WarrantyBudgetStatus.Queued:
                switch (next)
                {
                    case WarrantyBudgetStatus.Done:
                        return true;
                }
                return false;
            case WarrantyBudgetStatus.Running:
                return next == WarrantyBudgetStatus.Done;
            case WarrantyBudgetStatus.Done:
                return next == WarrantyBudgetStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WarrantyBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ceiling + (double)WindowDays, 2);
    }

    public WarrantyBudget Copy()
    {
        return new WarrantyBudget(
            Code,
            Name,
            Visible,
            LeaveDays,
            Reviewer,
            CreatedAt,
            Ceiling,
            Source,
            WindowDays,
            Category,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
