namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched garnishment metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedGarnishmentMetricService(IBatchedGarnishmentMetricStore Store, BatchedGarnishmentMetricValidator Validator, IClock Clock, List<string> Journal, BatchedGarnishmentMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched garnishment metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedGarnishmentMetricStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched garnishment metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedGarnishmentMetricStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched garnishment metric {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of batched garnishment metric workflow.</summary>
public static class BatchedGarnishmentMetricWorkflow
{
    public static BatchedGarnishmentMetricService NewBatchedGarnishmentMetricService(IBatchedGarnishmentMetricStore store, IClock clock)
    {
        return new BatchedGarnishmentMetricService(
            store,
            BatchedGarnishmentMetricRules.DefaultBatchedGarnishmentMetricValidator(),
            clock,
            new List<string>(),
            BatchedGarnishmentMetricEngine.DefaultBatchedGarnishmentMetricEngine());
    }
}
