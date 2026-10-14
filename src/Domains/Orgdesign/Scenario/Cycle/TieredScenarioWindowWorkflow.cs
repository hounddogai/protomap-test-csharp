namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered scenario windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredScenarioWindowDesk(ITieredScenarioWindowStore Store, TieredScenarioWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredScenarioWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredScenarioWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of tiered scenario window workflow.</summary>
public static class TieredScenarioWindowWorkflow
{
    public static TieredScenarioWindowDesk NewTieredScenarioWindowDesk(ITieredScenarioWindowStore store, IClock clock)
    {
        return new TieredScenarioWindowDesk(
            store,
            TieredScenarioWindowRules.DefaultTieredScenarioWindowValidator(),
            clock,
            new List<string>());
    }
}
