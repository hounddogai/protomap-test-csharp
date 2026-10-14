namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched cost model batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedCostModelBatchManager(IBatchedCostModelBatchStore Store, BatchedCostModelBatchValidator Validator, IClock Clock, List<string> Journal, BatchedCostModelBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched cost model batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedCostModelBatchStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched cost model batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedCostModelBatchStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched cost model batch {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of batched cost model batch workflow.</summary>
public static class BatchedCostModelBatchWorkflow
{
    public static BatchedCostModelBatchManager NewBatchedCostModelBatchManager(IBatchedCostModelBatchStore store, IClock clock, Registry flags)
    {
        return new BatchedCostModelBatchManager(
            store,
            BatchedCostModelBatchRules.DefaultBatchedCostModelBatchValidator(),
            clock,
            new List<string>(),
            BatchedCostModelBatchEngine.DefaultBatchedCostModelBatchEngine(flags));
    }
}
