namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled balance drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledBalanceDraftCoordinator(IScheduledBalanceDraftStore Store, ScheduledBalanceDraftValidator Validator, IClock Clock, List<string> Journal, ScheduledBalanceDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled balance draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledBalanceDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled balance draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledBalanceDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled balance draft {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of scheduled balance draft workflow.</summary>
public static class ScheduledBalanceDraftWorkflow
{
    public static ScheduledBalanceDraftCoordinator NewScheduledBalanceDraftCoordinator(IScheduledBalanceDraftStore store, IClock clock, Registry flags)
    {
        return new ScheduledBalanceDraftCoordinator(
            store,
            ScheduledBalanceDraftRules.DefaultScheduledBalanceDraftValidator(),
            clock,
            new List<string>(),
            ScheduledBalanceDraftEngine.DefaultScheduledBalanceDraftEngine(flags));
    }
}
