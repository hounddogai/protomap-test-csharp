namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered cost model entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredCostModelEntryService(ITieredCostModelEntryStore Store, TieredCostModelEntryValidator Validator, IClock Clock, List<string> Journal, TieredCostModelEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered cost model entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredCostModelEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered cost model entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredCostModelEntryStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered cost model entry {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of tiered cost model entry workflow.</summary>
public static class TieredCostModelEntryWorkflow
{
    public static TieredCostModelEntryService NewTieredCostModelEntryService(ITieredCostModelEntryStore store, IClock clock)
    {
        return new TieredCostModelEntryService(
            store,
            TieredCostModelEntryRules.DefaultTieredCostModelEntryValidator(),
            clock,
            new List<string>(),
            TieredCostModelEntryEngine.DefaultTieredCostModelEntryEngine());
    }
}
