namespace Acme.Workforce.Talent.SuccessionPlan.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan budget is in its life cycle.</summary>
public enum SuccessionPlanBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One succession plan budget, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanBudget(string code, string name, double rate, string notes, double minimum, string description, string reviewer, string currency, bool approved, string jobCode, bool visible, SuccessionPlanBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Notes { get; set; } = notes;
    public double Minimum { get; set; } = minimum;
    public string Description { get; set; } = description;
    public string Reviewer { get; set; } = reviewer;
    public string Currency { get; set; } = currency;
    public bool Approved { get; set; } = approved;
    public string JobCode { get; set; } = jobCode;
    public bool Visible { get; set; } = visible;
    public SuccessionPlanBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan budget in status open.</summary>
    public static SuccessionPlanBudget NewSuccessionPlanBudget(string code, string name)
    {
        return new SuccessionPlanBudget(
            code,
            name,
            0.0,
            "",
            0.0,
            "",
            "",
            "",
            false,
            "",
            false,
            SuccessionPlanBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanBudgetStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanBudgetStatus.Open:
                return next == SuccessionPlanBudgetStatus.Archived;
            case SuccessionPlanBudgetStatus.InReview:
                switch (next)
                {
                    case SuccessionPlanBudgetStatus.Reopened:
                        return true;
                }
                return false;
            case SuccessionPlanBudgetStatus.Resolved:
                switch (next)
                {
                    case SuccessionPlanBudgetStatus.Reopened:
                    case SuccessionPlanBudgetStatus.Archived:
                        return true;
                }
                return false;
            case SuccessionPlanBudgetStatus.Reopened:
                switch (next)
                {
                    case SuccessionPlanBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Minimum - Rate, 2);
    }

    public SuccessionPlanBudget Copy()
    {
        return new SuccessionPlanBudget(
            Code,
            Name,
            Rate,
            Notes,
            Minimum,
            Description,
            Reviewer,
            Currency,
            Approved,
            JobCode,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
