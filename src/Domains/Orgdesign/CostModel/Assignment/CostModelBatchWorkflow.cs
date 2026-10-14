namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves cost model batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelBatchService(ICostModelBatchStore Store, CostModelBatchValidator Validator, IClock Clock, List<string> Journal, CostModelBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelBatchStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelBatchStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model batch {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of cost model batch workflow.</summary>
public static class CostModelBatchWorkflow
{
    public static CostModelBatchService NewCostModelBatchService(ICostModelBatchStore store, IClock clock, Registry flags)
    {
        return new CostModelBatchService(
            store,
            CostModelBatchRules.DefaultCostModelBatchValidator(),
            clock,
            new List<string>(),
            CostModelBatchEngine.DefaultCostModelBatchEngine(flags));
    }
}
