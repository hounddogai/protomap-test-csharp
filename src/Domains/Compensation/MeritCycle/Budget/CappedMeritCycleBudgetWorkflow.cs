namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped merit cycle budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedMeritCycleBudgetManager(ICappedMeritCycleBudgetStore Store, CappedMeritCycleBudgetValidator Validator, IClock Clock, List<string> Journal, CappedMeritCycleBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped merit cycle budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedMeritCycleBudgetStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped merit cycle budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedMeritCycleBudgetStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped merit cycle budget {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of capped merit cycle budget workflow.</summary>
public static class CappedMeritCycleBudgetWorkflow
{
    public static CappedMeritCycleBudgetManager NewCappedMeritCycleBudgetManager(ICappedMeritCycleBudgetStore store, IClock clock, Registry flags)
    {
        return new CappedMeritCycleBudgetManager(
            store,
            CappedMeritCycleBudgetRules.DefaultCappedMeritCycleBudgetValidator(),
            clock,
            new List<string>(),
            CappedMeritCycleBudgetEngine.DefaultCappedMeritCycleBudgetEngine(flags));
    }
}
