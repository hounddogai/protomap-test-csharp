namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered scenario drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredScenarioDraftCoordinator(ITieredScenarioDraftStore Store, TieredScenarioDraftValidator Validator, IClock Clock, List<string> Journal, TieredScenarioDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredScenarioDraftStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredScenarioDraftStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario draft {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of tiered scenario draft workflow.</summary>
public static class TieredScenarioDraftWorkflow
{
    public static TieredScenarioDraftCoordinator NewTieredScenarioDraftCoordinator(ITieredScenarioDraftStore store, IClock clock, Registry flags)
    {
        return new TieredScenarioDraftCoordinator(
            store,
            TieredScenarioDraftRules.DefaultTieredScenarioDraftValidator(),
            clock,
            new List<string>(),
            TieredScenarioDraftEngine.DefaultTieredScenarioDraftEngine(flags));
    }
}
