namespace Acme.Workforce.Engagement.ActionPlan.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves action plan snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record ActionPlanSnapshotCoordinator(IActionPlanSnapshotStore Store, ActionPlanSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ActionPlanSnapshotStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"action plan snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ActionPlanSnapshotStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of action plan snapshot workflow.</summary>
public static class ActionPlanSnapshotWorkflow
{
    public static ActionPlanSnapshotCoordinator NewActionPlanSnapshotCoordinator(IActionPlanSnapshotStore store, IClock clock)
    {
        return new ActionPlanSnapshotCoordinator(
            store,
            ActionPlanSnapshotRules.DefaultActionPlanSnapshotValidator(),
            clock,
            new List<string>());
    }
}
