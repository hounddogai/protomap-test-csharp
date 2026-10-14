namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global equity grant drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalEquityGrantDraftCoordinator(IGlobalEquityGrantDraftStore Store, GlobalEquityGrantDraftValidator Validator, IClock Clock, List<string> Journal, GlobalEquityGrantDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global equity grant draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalEquityGrantDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global equity grant draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalEquityGrantDraftStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global equity grant draft {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of global equity grant draft workflow.</summary>
public static class GlobalEquityGrantDraftWorkflow
{
    public static GlobalEquityGrantDraftCoordinator NewGlobalEquityGrantDraftCoordinator(IGlobalEquityGrantDraftStore store, IClock clock, Registry flags)
    {
        return new GlobalEquityGrantDraftCoordinator(
            store,
            GlobalEquityGrantDraftRules.DefaultGlobalEquityGrantDraftValidator(),
            clock,
            new List<string>(),
            GlobalEquityGrantDraftEngine.DefaultGlobalEquityGrantDraftEngine(flags));
    }
}
