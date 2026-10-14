namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched adjustment rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedAdjustmentRuleManager(IBatchedAdjustmentRuleStore Store, BatchedAdjustmentRuleValidator Validator, IClock Clock, List<string> Journal, BatchedAdjustmentRuleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched adjustment rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAdjustmentRuleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched adjustment rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAdjustmentRuleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched adjustment rule {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of batched adjustment rule workflow.</summary>
public static class BatchedAdjustmentRuleWorkflow
{
    public static BatchedAdjustmentRuleManager NewBatchedAdjustmentRuleManager(IBatchedAdjustmentRuleStore store, IClock clock, Registry flags)
    {
        return new BatchedAdjustmentRuleManager(
            store,
            BatchedAdjustmentRuleRules.DefaultBatchedAdjustmentRuleValidator(),
            clock,
            new List<string>(),
            BatchedAdjustmentRuleEngine.DefaultBatchedAdjustmentRuleEngine(flags));
    }
}
