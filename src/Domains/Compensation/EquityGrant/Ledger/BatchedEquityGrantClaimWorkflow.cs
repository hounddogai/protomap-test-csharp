namespace Acme.Workforce.Compensation.EquityGrant.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched equity grant claims through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedEquityGrantClaimDesk(IBatchedEquityGrantClaimStore Store, BatchedEquityGrantClaimValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched equity grant claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedEquityGrantClaimStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched equity grant claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedEquityGrantClaimStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of batched equity grant claim workflow.</summary>
public static class BatchedEquityGrantClaimWorkflow
{
    public static BatchedEquityGrantClaimDesk NewBatchedEquityGrantClaimDesk(IBatchedEquityGrantClaimStore store, IClock clock)
    {
        return new BatchedEquityGrantClaimDesk(
            store,
            BatchedEquityGrantClaimRules.DefaultBatchedEquityGrantClaimValidator(),
            clock,
            new List<string>());
    }
}
