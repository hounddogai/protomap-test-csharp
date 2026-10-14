namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves access badge entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeEntryCoordinator(IAccessBadgeEntryStore Store, AccessBadgeEntryValidator Validator, IClock Clock, List<string> Journal, AccessBadgeEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge entry {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of access badge entry workflow.</summary>
public static class AccessBadgeEntryWorkflow
{
    public static AccessBadgeEntryCoordinator NewAccessBadgeEntryCoordinator(IAccessBadgeEntryStore store, IClock clock, Registry flags)
    {
        return new AccessBadgeEntryCoordinator(
            store,
            AccessBadgeEntryRules.DefaultAccessBadgeEntryValidator(),
            clock,
            new List<string>(),
            AccessBadgeEntryEngine.DefaultAccessBadgeEntryEngine(flags));
    }
}
