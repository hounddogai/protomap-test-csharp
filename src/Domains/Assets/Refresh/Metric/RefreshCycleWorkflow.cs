namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshCycleManager(IRefreshCycleStore Store, RefreshCycleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshCycleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshCycleStatus.Expired);
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

/// <summary>Functions of refresh cycle workflow.</summary>
public static class RefreshCycleWorkflow
{
    public static RefreshCycleManager NewRefreshCycleManager(IRefreshCycleStore store, IClock clock)
    {
        return new RefreshCycleManager(
            store,
            RefreshCycleRules.DefaultRefreshCycleValidator(),
            clock,
            new List<string>());
    }
}
