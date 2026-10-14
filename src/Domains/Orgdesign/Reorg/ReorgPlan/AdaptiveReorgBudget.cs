namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive reorg budget is in its life cycle.</summary>
public enum AdaptiveReorgBudgetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive reorg budget as the reorg team keeps it.</summary>
public sealed class AdaptiveReorgBudget(string code, string title, int startsAt, int windowDays, string description, int priority, string category, AdaptiveReorgBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public string Description { get; set; } = description;
    public int Priority { get; set; } = priority;
    public string Category { get; set; } = category;
    public AdaptiveReorgBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive reorg budget in status proposed.</summary>
    public static AdaptiveReorgBudget NewAdaptiveReorgBudget(string code, string title)
    {
        return new AdaptiveReorgBudget(code, title, 0, 0, "", 0, "", AdaptiveReorgBudgetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveReorgBudgetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveReorgBudgetStatus next)
    {
        switch (Status)
        {
            case AdaptiveReorgBudgetStatus.Proposed:
                switch (next)
                {
                    case AdaptiveReorgBudgetStatus.Approved:
                        return true;
                }
                return false;
            case AdaptiveReorgBudgetStatus.Approved:
                return next == AdaptiveReorgBudgetStatus.Rejected;
            case AdaptiveReorgBudgetStatus.Rejected:
                switch (next)
                {
                    case AdaptiveReorgBudgetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveReorgBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive reorg budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive reorg budget"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)WindowDays + (double)StartsAt, 2);
    }

    public AdaptiveReorgBudget Copy()
    {
        return new AdaptiveReorgBudget(Code, Title, StartsAt, WindowDays, Description, Priority, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
