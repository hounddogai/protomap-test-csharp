namespace Acme.Workforce.Orgdesign.CostModel.ModelGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rolling cost model batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingCostModelBatchManager(IRollingCostModelBatchStore Store, RollingCostModelBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling cost model batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingCostModelBatchStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling cost model batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingCostModelBatchStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of rolling cost model batch workflow.</summary>
public static class RollingCostModelBatchWorkflow
{
    public static RollingCostModelBatchManager NewRollingCostModelBatchManager(IRollingCostModelBatchStore store, IClock clock)
    {
        return new RollingCostModelBatchManager(
            store,
            RollingCostModelBatchRules.DefaultRollingCostModelBatchValidator(),
            clock,
            new List<string>());
    }
}
