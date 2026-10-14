namespace Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy headcount budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyHeadcountBudgetService(ILegacyHeadcountBudgetStore Store, LegacyHeadcountBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy headcount budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyHeadcountBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy headcount budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyHeadcountBudgetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of legacy headcount budget workflow.</summary>
public static class LegacyHeadcountBudgetWorkflow
{
    public static LegacyHeadcountBudgetService NewLegacyHeadcountBudgetService(ILegacyHeadcountBudgetStore store, IClock clock)
    {
        return new LegacyHeadcountBudgetService(
            store,
            LegacyHeadcountBudgetRules.DefaultLegacyHeadcountBudgetValidator(),
            clock,
            new List<string>());
    }
}
