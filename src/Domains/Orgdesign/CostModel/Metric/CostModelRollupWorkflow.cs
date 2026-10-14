namespace Acme.Workforce.Orgdesign.CostModel.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves cost model rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelRollupDesk(ICostModelRollupStore Store, CostModelRollupValidator Validator, IClock Clock, List<string> Journal, CostModelRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model rollup {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of cost model rollup workflow.</summary>
public static class CostModelRollupWorkflow
{
    public static CostModelRollupDesk NewCostModelRollupDesk(ICostModelRollupStore store, IClock clock)
    {
        return new CostModelRollupDesk(
            store,
            CostModelRollupRules.DefaultCostModelRollupValidator(),
            clock,
            new List<string>(),
            CostModelRollupEngine.DefaultCostModelRollupEngine());
    }
}
