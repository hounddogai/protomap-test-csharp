namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered scenario policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredScenarioPolicyService(ITieredScenarioPolicyStore Store, TieredScenarioPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredScenarioPolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered scenario policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredScenarioPolicyStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of tiered scenario policy workflow.</summary>
public static class TieredScenarioPolicyWorkflow
{
    public static TieredScenarioPolicyService NewTieredScenarioPolicyService(ITieredScenarioPolicyStore store, IClock clock)
    {
        return new TieredScenarioPolicyService(
            store,
            TieredScenarioPolicyRules.DefaultTieredScenarioPolicyValidator(),
            clock,
            new List<string>());
    }
}
