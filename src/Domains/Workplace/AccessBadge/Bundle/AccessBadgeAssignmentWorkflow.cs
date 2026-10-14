namespace Acme.Workforce.Workplace.AccessBadge.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves access badge assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeAssignmentCoordinator(IAccessBadgeAssignmentStore Store, AccessBadgeAssignmentValidator Validator, IClock Clock, List<string> Journal, AccessBadgeAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge assignment {code} not found");
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

/// <summary>Functions of access badge assignment workflow.</summary>
public static class AccessBadgeAssignmentWorkflow
{
    public static AccessBadgeAssignmentCoordinator NewAccessBadgeAssignmentCoordinator(IAccessBadgeAssignmentStore store, IClock clock, Registry flags)
    {
        return new AccessBadgeAssignmentCoordinator(
            store,
            AccessBadgeAssignmentRules.DefaultAccessBadgeAssignmentValidator(),
            clock,
            new List<string>(),
            AccessBadgeAssignmentEngine.DefaultAccessBadgeAssignmentEngine(flags));
    }
}
