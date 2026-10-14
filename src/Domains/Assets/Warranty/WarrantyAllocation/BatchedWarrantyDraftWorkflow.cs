namespace Acme.Workforce.Assets.Warranty.WarrantyAllocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched warranty drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedWarrantyDraftService(IBatchedWarrantyDraftStore Store, BatchedWarrantyDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedWarrantyDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedWarrantyDraftStatus.Stopped);
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

/// <summary>Functions of batched warranty draft workflow.</summary>
public static class BatchedWarrantyDraftWorkflow
{
    public static BatchedWarrantyDraftService NewBatchedWarrantyDraftService(IBatchedWarrantyDraftStore store, IClock clock)
    {
        return new BatchedWarrantyDraftService(
            store,
            BatchedWarrantyDraftRules.DefaultBatchedWarrantyDraftValidator(),
            clock,
            new List<string>());
    }
}
