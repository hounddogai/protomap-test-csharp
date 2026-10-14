namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched warranty bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedWarrantyBundleCoordinator(IBatchedWarrantyBundleStore Store, BatchedWarrantyBundleValidator Validator, IClock Clock, List<string> Journal, BatchedWarrantyBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedWarrantyBundleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedWarrantyBundleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched warranty bundle {code} not found");
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

/// <summary>Functions of batched warranty bundle workflow.</summary>
public static class BatchedWarrantyBundleWorkflow
{
    public static BatchedWarrantyBundleCoordinator NewBatchedWarrantyBundleCoordinator(IBatchedWarrantyBundleStore store, IClock clock, Registry flags)
    {
        return new BatchedWarrantyBundleCoordinator(
            store,
            BatchedWarrantyBundleRules.DefaultBatchedWarrantyBundleValidator(),
            clock,
            new List<string>(),
            BatchedWarrantyBundleEngine.DefaultBatchedWarrantyBundleEngine(flags));
    }
}
