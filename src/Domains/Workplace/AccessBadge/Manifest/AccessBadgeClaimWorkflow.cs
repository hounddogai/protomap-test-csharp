namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeClaimDesk(IAccessBadgeClaimStore Store, AccessBadgeClaimValidator Validator, IClock Clock, List<string> Journal, AccessBadgeClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeClaimStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeClaimStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge claim {code} not found");
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

/// <summary>Functions of access badge claim workflow.</summary>
public static class AccessBadgeClaimWorkflow
{
    public static AccessBadgeClaimDesk NewAccessBadgeClaimDesk(IAccessBadgeClaimStore store, IClock clock)
    {
        return new AccessBadgeClaimDesk(
            store,
            AccessBadgeClaimRules.DefaultAccessBadgeClaimValidator(),
            clock,
            new List<string>(),
            AccessBadgeClaimEngine.DefaultAccessBadgeClaimEngine());
    }
}
