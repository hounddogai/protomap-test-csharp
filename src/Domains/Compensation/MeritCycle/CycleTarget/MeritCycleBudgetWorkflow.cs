namespace Acme.Workforce.Compensation.MeritCycle.CycleTarget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves merit cycle budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleBudgetService(IMeritCycleBudgetStore Store, MeritCycleBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleBudgetStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleBudgetStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of merit cycle budget workflow.</summary>
public static class MeritCycleBudgetWorkflow
{
    public static MeritCycleBudgetService NewMeritCycleBudgetService(IMeritCycleBudgetStore store, IClock clock)
    {
        return new MeritCycleBudgetService(
            store,
            MeritCycleBudgetRules.DefaultMeritCycleBudgetValidator(),
            clock,
            new List<string>());
    }
}
