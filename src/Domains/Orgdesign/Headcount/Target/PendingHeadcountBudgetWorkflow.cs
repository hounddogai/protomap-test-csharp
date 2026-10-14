namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending headcount budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingHeadcountBudgetService(IPendingHeadcountBudgetStore Store, PendingHeadcountBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending headcount budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingHeadcountBudgetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending headcount budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingHeadcountBudgetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of pending headcount budget workflow.</summary>
public static class PendingHeadcountBudgetWorkflow
{
    public static PendingHeadcountBudgetService NewPendingHeadcountBudgetService(IPendingHeadcountBudgetStore store, IClock clock)
    {
        return new PendingHeadcountBudgetService(
            store,
            PendingHeadcountBudgetRules.DefaultPendingHeadcountBudgetValidator(),
            clock,
            new List<string>());
    }
}
