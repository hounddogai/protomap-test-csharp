namespace Acme.Workforce.Absence.Accrual.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched accrual metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedAccrualMetricWorkflow(IBatchedAccrualMetricStore Store, BatchedAccrualMetricValidator Validator, IClock Clock, List<string> Journal)
{
    public static BatchedAccrualMetricWorkflow NewBatchedAccrualMetricWorkflow(IBatchedAccrualMetricStore store, IClock clock)
    {
        return new BatchedAccrualMetricWorkflow(
            store,
            BatchedAccrualMetricRules.DefaultBatchedAccrualMetricValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched accrual metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAccrualMetricStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched accrual metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAccrualMetricStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
