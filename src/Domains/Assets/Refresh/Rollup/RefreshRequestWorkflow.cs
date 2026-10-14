namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshRequestCoordinator(IRefreshRequestStore Store, RefreshRequestValidator Validator, IClock Clock, List<string> Journal, RefreshRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshRequestStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshRequestStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh request {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of refresh request workflow.</summary>
public static class RefreshRequestWorkflow
{
    public static RefreshRequestCoordinator NewRefreshRequestCoordinator(IRefreshRequestStore store, IClock clock)
    {
        return new RefreshRequestCoordinator(
            store,
            RefreshRequestRules.DefaultRefreshRequestValidator(),
            clock,
            new List<string>(),
            RefreshRequestEngine.DefaultRefreshRequestEngine());
    }
}
