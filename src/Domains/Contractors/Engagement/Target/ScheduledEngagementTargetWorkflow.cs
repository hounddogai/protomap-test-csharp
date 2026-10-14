namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled engagement targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledEngagementTargetDesk(IScheduledEngagementTargetStore Store, ScheduledEngagementTargetValidator Validator, IClock Clock, List<string> Journal, ScheduledEngagementTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled engagement target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledEngagementTargetStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled engagement target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledEngagementTargetStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled engagement target {code} not found");
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

/// <summary>Functions of scheduled engagement target workflow.</summary>
public static class ScheduledEngagementTargetWorkflow
{
    public static ScheduledEngagementTargetDesk NewScheduledEngagementTargetDesk(IScheduledEngagementTargetStore store, IClock clock)
    {
        return new ScheduledEngagementTargetDesk(
            store,
            ScheduledEngagementTargetRules.DefaultScheduledEngagementTargetValidator(),
            clock,
            new List<string>(),
            ScheduledEngagementTargetEngine.DefaultScheduledEngagementTargetEngine());
    }
}
