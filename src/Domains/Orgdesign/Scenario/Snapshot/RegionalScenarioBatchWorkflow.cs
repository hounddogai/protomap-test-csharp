namespace Acme.Workforce.Orgdesign.Scenario.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional scenario batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalScenarioBatchService(IRegionalScenarioBatchStore Store, RegionalScenarioBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional scenario batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalScenarioBatchStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional scenario batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalScenarioBatchStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of regional scenario batch workflow.</summary>
public static class RegionalScenarioBatchWorkflow
{
    public static RegionalScenarioBatchService NewRegionalScenarioBatchService(IRegionalScenarioBatchStore store, IClock clock)
    {
        return new RegionalScenarioBatchService(
            store,
            RegionalScenarioBatchRules.DefaultRegionalScenarioBatchValidator(),
            clock,
            new List<string>());
    }
}
