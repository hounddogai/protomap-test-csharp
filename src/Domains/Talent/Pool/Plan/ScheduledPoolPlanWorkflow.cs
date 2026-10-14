namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pool plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledPoolPlanDesk(IScheduledPoolPlanStore Store, ScheduledPoolPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pool plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPoolPlanStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pool plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPoolPlanStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of scheduled pool plan workflow.</summary>
public static class ScheduledPoolPlanWorkflow
{
    public static ScheduledPoolPlanDesk NewScheduledPoolPlanDesk(IScheduledPoolPlanStore store, IClock clock)
    {
        return new ScheduledPoolPlanDesk(
            store,
            ScheduledPoolPlanRules.DefaultScheduledPoolPlanValidator(),
            clock,
            new List<string>());
    }
}
