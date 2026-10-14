namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeExceptionManager(IAccessBadgeExceptionStore Store, AccessBadgeExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeExceptionStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeExceptionStatus.Closed);
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

/// <summary>Functions of access badge exception workflow.</summary>
public static class AccessBadgeExceptionWorkflow
{
    public static AccessBadgeExceptionManager NewAccessBadgeExceptionManager(IAccessBadgeExceptionStore store, IClock clock)
    {
        return new AccessBadgeExceptionManager(
            store,
            AccessBadgeExceptionRules.DefaultAccessBadgeExceptionValidator(),
            clock,
            new List<string>());
    }
}
