namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched asset return budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedAssetReturnBudgetCoordinator(IBatchedAssetReturnBudgetStore Store, BatchedAssetReturnBudgetValidator Validator, IClock Clock, List<string> Journal, BatchedAssetReturnBudgetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched asset return budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAssetReturnBudgetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched asset return budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAssetReturnBudgetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched asset return budget {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of batched asset return budget workflow.</summary>
public static class BatchedAssetReturnBudgetWorkflow
{
    public static BatchedAssetReturnBudgetCoordinator NewBatchedAssetReturnBudgetCoordinator(IBatchedAssetReturnBudgetStore store, IClock clock, Registry flags)
    {
        return new BatchedAssetReturnBudgetCoordinator(
            store,
            BatchedAssetReturnBudgetRules.DefaultBatchedAssetReturnBudgetValidator(),
            clock,
            new List<string>(),
            BatchedAssetReturnBudgetEngine.DefaultBatchedAssetReturnBudgetEngine(flags));
    }
}
