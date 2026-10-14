namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshScoreCoordinator(IRefreshScoreStore Store, RefreshScoreValidator Validator, IClock Clock, List<string> Journal, RefreshScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshScoreStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshScoreStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh score {code} not found");
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

/// <summary>Functions of refresh score workflow.</summary>
public static class RefreshScoreWorkflow
{
    public static RefreshScoreCoordinator NewRefreshScoreCoordinator(IRefreshScoreStore store, IClock clock)
    {
        return new RefreshScoreCoordinator(
            store,
            RefreshScoreRules.DefaultRefreshScoreValidator(),
            clock,
            new List<string>(),
            RefreshScoreEngine.DefaultRefreshScoreEngine());
    }
}
