namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled action plan slots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledActionPlanSlotManager(IScheduledActionPlanSlotStore Store, ScheduledActionPlanSlotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled action plan slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledActionPlanSlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled action plan slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledActionPlanSlotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of scheduled action plan slot workflow.</summary>
public static class ScheduledActionPlanSlotWorkflow
{
    public static ScheduledActionPlanSlotManager NewScheduledActionPlanSlotManager(IScheduledActionPlanSlotStore store, IClock clock)
    {
        return new ScheduledActionPlanSlotManager(
            store,
            ScheduledActionPlanSlotRules.DefaultScheduledActionPlanSlotValidator(),
            clock,
            new List<string>());
    }
}
