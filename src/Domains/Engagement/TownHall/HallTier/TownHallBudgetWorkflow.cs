namespace Acme.Workforce.Engagement.TownHall.HallTier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves town hall budgets through their life cycle and keeps a journal of what it did.</summary>
public sealed record TownHallBudgetDesk(ITownHallBudgetStore Store, TownHallBudgetValidator Validator, IClock Clock, List<string> Journal, TownHallBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TownHallBudgetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TownHallBudgetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"town hall budget {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of town hall budget workflow.</summary>
public static class TownHallBudgetWorkflow
{
    public static TownHallBudgetDesk NewTownHallBudgetDesk(ITownHallBudgetStore store, IClock clock, Registry flags)
    {
        return new TownHallBudgetDesk(
            store,
            TownHallBudgetRules.DefaultTownHallBudgetValidator(),
            clock,
            new List<string>(),
            TownHallBudgetEngine.DefaultTownHallBudgetEngine(flags));
    }
}
