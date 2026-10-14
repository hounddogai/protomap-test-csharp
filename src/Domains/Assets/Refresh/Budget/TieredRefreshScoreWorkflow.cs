namespace Acme.Workforce.Assets.Refresh.Budget;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered refresh scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredRefreshScoreCoordinator(ITieredRefreshScoreStore Store, TieredRefreshScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered refresh score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredRefreshScoreStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered refresh score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredRefreshScoreStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of tiered refresh score workflow.</summary>
public static class TieredRefreshScoreWorkflow
{
    public static TieredRefreshScoreCoordinator NewTieredRefreshScoreCoordinator(ITieredRefreshScoreStore store, IClock clock)
    {
        return new TieredRefreshScoreCoordinator(
            store,
            TieredRefreshScoreRules.DefaultTieredRefreshScoreValidator(),
            clock,
            new List<string>());
    }
}
