namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched refresh windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedRefreshWindowCoordinator(IBatchedRefreshWindowStore Store, BatchedRefreshWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched refresh window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedRefreshWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched refresh window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedRefreshWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of batched refresh window workflow.</summary>
public static class BatchedRefreshWindowWorkflow
{
    public static BatchedRefreshWindowCoordinator NewBatchedRefreshWindowCoordinator(IBatchedRefreshWindowStore store, IClock clock)
    {
        return new BatchedRefreshWindowCoordinator(
            store,
            BatchedRefreshWindowRules.DefaultBatchedRefreshWindowValidator(),
            clock,
            new List<string>());
    }
}
