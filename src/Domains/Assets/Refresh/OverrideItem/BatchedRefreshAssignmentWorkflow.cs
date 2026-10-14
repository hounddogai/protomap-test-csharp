namespace Acme.Workforce.Assets.Refresh.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched refresh assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedRefreshAssignmentWorkflow(IBatchedRefreshAssignmentStore Store, BatchedRefreshAssignmentValidator Validator, IClock Clock, List<string> Journal, BatchedRefreshAssignmentEngine Engine)
{
    public static BatchedRefreshAssignmentWorkflow NewBatchedRefreshAssignmentWorkflow(IBatchedRefreshAssignmentStore store, IClock clock, Registry flags)
    {
        return new BatchedRefreshAssignmentWorkflow(
            store,
            BatchedRefreshAssignmentRules.DefaultBatchedRefreshAssignmentValidator(),
            clock,
            new List<string>(),
            BatchedRefreshAssignmentEngine.DefaultBatchedRefreshAssignmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched refresh assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedRefreshAssignmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched refresh assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedRefreshAssignmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched refresh assignment {code} not found");
        }
        var result = Engine.Run(item);
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
