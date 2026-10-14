namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched bonus pool thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedBonusPoolThresholdCoordinator(IBatchedBonusPoolThresholdStore Store, BatchedBonusPoolThresholdValidator Validator, IClock Clock, List<string> Journal, BatchedBonusPoolThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched bonus pool threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedBonusPoolThresholdStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched bonus pool threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedBonusPoolThresholdStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched bonus pool threshold {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of batched bonus pool threshold workflow.</summary>
public static class BatchedBonusPoolThresholdWorkflow
{
    public static BatchedBonusPoolThresholdCoordinator NewBatchedBonusPoolThresholdCoordinator(IBatchedBonusPoolThresholdStore store, IClock clock, Registry flags)
    {
        return new BatchedBonusPoolThresholdCoordinator(
            store,
            BatchedBonusPoolThresholdRules.DefaultBatchedBonusPoolThresholdValidator(),
            clock,
            new List<string>(),
            BatchedBonusPoolThresholdEngine.DefaultBatchedBonusPoolThresholdEngine(flags));
    }
}
