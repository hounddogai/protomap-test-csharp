namespace Acme.Workforce.Contractors.Engagement.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched engagement variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedEngagementVarianceWorkflow(IBatchedEngagementVarianceStore Store, BatchedEngagementVarianceValidator Validator, IClock Clock, List<string> Journal, BatchedEngagementVarianceEngine Engine)
{
    public static BatchedEngagementVarianceWorkflow NewBatchedEngagementVarianceWorkflow(IBatchedEngagementVarianceStore store, IClock clock)
    {
        return new BatchedEngagementVarianceWorkflow(
            store,
            BatchedEngagementVarianceRules.DefaultBatchedEngagementVarianceValidator(),
            clock,
            new List<string>(),
            BatchedEngagementVarianceEngine.DefaultBatchedEngagementVarianceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched engagement variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedEngagementVarianceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched engagement variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedEngagementVarianceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched engagement variance {code} not found");
        }
        var result = Engine.Compute(item);
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
