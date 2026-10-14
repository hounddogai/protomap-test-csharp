namespace Acme.Workforce.Compensation.EquityGrant.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantBatchService(IEquityGrantBatchStore Store, EquityGrantBatchValidator Validator, IClock Clock, List<string> Journal, EquityGrantBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantBatchStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantBatchStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant batch {code} not found");
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

/// <summary>Functions of equity grant batch workflow.</summary>
public static class EquityGrantBatchWorkflow
{
    public static EquityGrantBatchService NewEquityGrantBatchService(IEquityGrantBatchStore store, IClock clock)
    {
        return new EquityGrantBatchService(
            store,
            EquityGrantBatchRules.DefaultEquityGrantBatchValidator(),
            clock,
            new List<string>(),
            EquityGrantBatchEngine.DefaultEquityGrantBatchEngine());
    }
}
