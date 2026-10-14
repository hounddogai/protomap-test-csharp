namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched adjustment requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedAdjustmentRequestWorkflow(IBatchedAdjustmentRequestStore Store, BatchedAdjustmentRequestValidator Validator, IClock Clock, List<string> Journal)
{
    public static BatchedAdjustmentRequestWorkflow NewBatchedAdjustmentRequestWorkflow(IBatchedAdjustmentRequestStore store, IClock clock)
    {
        return new BatchedAdjustmentRequestWorkflow(
            store,
            BatchedAdjustmentRequestRules.DefaultBatchedAdjustmentRequestValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched adjustment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAdjustmentRequestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched adjustment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAdjustmentRequestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
