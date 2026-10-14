namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched pay run metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedPayRunMetricService(IBatchedPayRunMetricStore Store, BatchedPayRunMetricValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay run metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedPayRunMetricStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched pay run metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedPayRunMetricStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of batched pay run metric workflow.</summary>
public static class BatchedPayRunMetricWorkflow
{
    public static BatchedPayRunMetricService NewBatchedPayRunMetricService(IBatchedPayRunMetricStore store, IClock clock)
    {
        return new BatchedPayRunMetricService(
            store,
            BatchedPayRunMetricRules.DefaultBatchedPayRunMetricValidator(),
            clock,
            new List<string>());
    }
}
