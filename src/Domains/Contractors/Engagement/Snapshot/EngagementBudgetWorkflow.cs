namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves engagement budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record EngagementBudgetService(IEngagementBudgetStore Store, EngagementBudgetValidator Validator, IClock Clock, List<string> Journal, EngagementBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EngagementBudgetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EngagementBudgetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"engagement budget {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of engagement budget workflow.</summary>
public static class EngagementBudgetWorkflow
{
    public static EngagementBudgetService NewEngagementBudgetService(IEngagementBudgetStore store, IClock clock, Registry flags)
    {
        return new EngagementBudgetService(
            store,
            EngagementBudgetRules.DefaultEngagementBudgetValidator(),
            clock,
            new List<string>(),
            EngagementBudgetEngine.DefaultEngagementBudgetEngine(flags));
    }
}
