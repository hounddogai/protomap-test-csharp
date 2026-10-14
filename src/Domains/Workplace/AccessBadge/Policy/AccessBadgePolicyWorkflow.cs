namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgePolicyCoordinator(IAccessBadgePolicyStore Store, AccessBadgePolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgePolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgePolicyStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of access badge policy workflow.</summary>
public static class AccessBadgePolicyWorkflow
{
    public static AccessBadgePolicyCoordinator NewAccessBadgePolicyCoordinator(IAccessBadgePolicyStore store, IClock clock)
    {
        return new AccessBadgePolicyCoordinator(
            store,
            AccessBadgePolicyRules.DefaultAccessBadgePolicyValidator(),
            clock,
            new List<string>());
    }
}
