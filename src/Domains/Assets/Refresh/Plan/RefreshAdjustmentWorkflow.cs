namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves refresh adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshAdjustmentService(IRefreshAdjustmentStore Store, RefreshAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshAdjustmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshAdjustmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of refresh adjustment workflow.</summary>
public static class RefreshAdjustmentWorkflow
{
    public static RefreshAdjustmentService NewRefreshAdjustmentService(IRefreshAdjustmentStore store, IClock clock)
    {
        return new RefreshAdjustmentService(
            store,
            RefreshAdjustmentRules.DefaultRefreshAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
