namespace Acme.Workforce.Contractors.Engagement.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched engagement exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedEngagementExceptionWorkflow(IBatchedEngagementExceptionStore Store, BatchedEngagementExceptionValidator Validator, IClock Clock, List<string> Journal, BatchedEngagementExceptionEngine Engine)
{
    public static BatchedEngagementExceptionWorkflow NewBatchedEngagementExceptionWorkflow(IBatchedEngagementExceptionStore store, IClock clock)
    {
        return new BatchedEngagementExceptionWorkflow(
            store,
            BatchedEngagementExceptionRules.DefaultBatchedEngagementExceptionValidator(),
            clock,
            new List<string>(),
            BatchedEngagementExceptionEngine.DefaultBatchedEngagementExceptionEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched engagement exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedEngagementExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched engagement exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedEngagementExceptionStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched engagement exception {code} not found");
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
