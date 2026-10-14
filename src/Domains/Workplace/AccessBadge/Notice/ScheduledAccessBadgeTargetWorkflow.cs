namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled access badge targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAccessBadgeTargetCoordinator(IScheduledAccessBadgeTargetStore Store, ScheduledAccessBadgeTargetValidator Validator, IClock Clock, List<string> Journal, ScheduledAccessBadgeTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled access badge target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAccessBadgeTargetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled access badge target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAccessBadgeTargetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled access badge target {code} not found");
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

/// <summary>Functions of scheduled access badge target workflow.</summary>
public static class ScheduledAccessBadgeTargetWorkflow
{
    public static ScheduledAccessBadgeTargetCoordinator NewScheduledAccessBadgeTargetCoordinator(IScheduledAccessBadgeTargetStore store, IClock clock, Registry flags)
    {
        return new ScheduledAccessBadgeTargetCoordinator(
            store,
            ScheduledAccessBadgeTargetRules.DefaultScheduledAccessBadgeTargetValidator(),
            clock,
            new List<string>(),
            ScheduledAccessBadgeTargetEngine.DefaultScheduledAccessBadgeTargetEngine(flags));
    }
}
