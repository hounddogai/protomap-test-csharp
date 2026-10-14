namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending policy adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPolicyAdjustmentService(IPendingPolicyAdjustmentStore Store, PendingPolicyAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending policy adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPolicyAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending policy adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPolicyAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of pending policy adjustment workflow.</summary>
public static class PendingPolicyAdjustmentWorkflow
{
    public static PendingPolicyAdjustmentService NewPendingPolicyAdjustmentService(IPendingPolicyAdjustmentStore store, IClock clock)
    {
        return new PendingPolicyAdjustmentService(
            store,
            PendingPolicyAdjustmentRules.DefaultPendingPolicyAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
