namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched access badge targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedAccessBadgeTargetWorkflow(IBatchedAccessBadgeTargetStore Store, BatchedAccessBadgeTargetValidator Validator, IClock Clock, List<string> Journal, BatchedAccessBadgeTargetEngine Engine)
{
    public static BatchedAccessBadgeTargetWorkflow NewBatchedAccessBadgeTargetWorkflow(IBatchedAccessBadgeTargetStore store, IClock clock, Registry flags)
    {
        return new BatchedAccessBadgeTargetWorkflow(
            store,
            BatchedAccessBadgeTargetRules.DefaultBatchedAccessBadgeTargetValidator(),
            clock,
            new List<string>(),
            BatchedAccessBadgeTargetEngine.DefaultBatchedAccessBadgeTargetEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched access badge target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedAccessBadgeTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched access badge target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedAccessBadgeTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched access badge target {code} not found");
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
