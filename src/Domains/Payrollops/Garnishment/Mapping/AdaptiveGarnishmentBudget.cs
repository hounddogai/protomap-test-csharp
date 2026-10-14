namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive garnishment budget is in its life cycle.</summary>
public enum AdaptiveGarnishmentBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One adaptive garnishment budget, with what the planners need to act on it.</summary>
public sealed class AdaptiveGarnishmentBudget(string code, string title, int leaveDays, int priority, string country, string reviewer, int startsAt, double rate, string grade, int attempts, AdaptiveGarnishmentBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public int Priority { get; set; } = priority;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public int StartsAt { get; set; } = startsAt;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public int Attempts { get; set; } = attempts;
    public AdaptiveGarnishmentBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive garnishment budget in status idle.</summary>
    public static AdaptiveGarnishmentBudget NewAdaptiveGarnishmentBudget(string code, string title)
    {
        return new AdaptiveGarnishmentBudget(
            code,
            title,
            0,
            0,
            "",
            "",
            0,
            0.0,
            "",
            0,
            AdaptiveGarnishmentBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveGarnishmentBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveGarnishmentBudgetStatus next)
    {
        switch (Status)
        {
            case AdaptiveGarnishmentBudgetStatus.Idle:
                switch (next)
                {
                    case AdaptiveGarnishmentBudgetStatus.Ready:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentBudgetStatus.Warming:
                switch (next)
                {
                    case AdaptiveGarnishmentBudgetStatus.Draining:
                        return true;
                }
                return false;
            case AdaptiveGarnishmentBudgetStatus.Ready:
                return next == AdaptiveGarnishmentBudgetStatus.Draining || next == AdaptiveGarnishmentBudgetStatus.Stopped;
            case AdaptiveGarnishmentBudgetStatus.Draining:
                switch (next)
                {
                    case AdaptiveGarnishmentBudgetStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveGarnishmentBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive garnishment budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"adaptive garnishment budget"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Priority * Rate, 2);
    }

    public AdaptiveGarnishmentBudget Copy()
    {
        return new AdaptiveGarnishmentBudget(
            Code,
            Title,
            LeaveDays,
            Priority,
            Country,
            Reviewer,
            StartsAt,
            Rate,
            Grade,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
