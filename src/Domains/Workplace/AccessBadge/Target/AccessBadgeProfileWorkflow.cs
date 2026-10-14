namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeProfileService(IAccessBadgeProfileStore Store, AccessBadgeProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeProfileStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of access badge profile workflow.</summary>
public static class AccessBadgeProfileWorkflow
{
    public static AccessBadgeProfileService NewAccessBadgeProfileService(IAccessBadgeProfileStore store, IClock clock)
    {
        return new AccessBadgeProfileService(
            store,
            AccessBadgeProfileRules.DefaultAccessBadgeProfileValidator(),
            clock,
            new List<string>());
    }
}
