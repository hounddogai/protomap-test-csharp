namespace Acme.Workforce.Workplace.AccessBadge.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves access badge tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeTierManager(IAccessBadgeTierStore Store, AccessBadgeTierValidator Validator, IClock Clock, List<string> Journal, AccessBadgeTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge tier {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of access badge tier workflow.</summary>
public static class AccessBadgeTierWorkflow
{
    public static AccessBadgeTierManager NewAccessBadgeTierManager(IAccessBadgeTierStore store, IClock clock, Registry flags)
    {
        return new AccessBadgeTierManager(
            store,
            AccessBadgeTierRules.DefaultAccessBadgeTierValidator(),
            clock,
            new List<string>(),
            AccessBadgeTierEngine.DefaultAccessBadgeTierEngine(flags));
    }
}
