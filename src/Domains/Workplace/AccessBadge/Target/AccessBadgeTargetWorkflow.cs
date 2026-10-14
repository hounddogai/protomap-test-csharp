namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeTargetManager(IAccessBadgeTargetStore Store, AccessBadgeTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeTargetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeTargetStatus.Closed);
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

/// <summary>Functions of access badge target workflow.</summary>
public static class AccessBadgeTargetWorkflow
{
    public static AccessBadgeTargetManager NewAccessBadgeTargetManager(IAccessBadgeTargetStore store, IClock clock)
    {
        return new AccessBadgeTargetManager(
            store,
            AccessBadgeTargetRules.DefaultAccessBadgeTargetValidator(),
            clock,
            new List<string>());
    }
}
