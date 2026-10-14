namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves access badge mappings through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeMappingManager(IAccessBadgeMappingStore Store, AccessBadgeMappingValidator Validator, IClock Clock, List<string> Journal, AccessBadgeMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeMappingStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeMappingStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge mapping {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of access badge mapping workflow.</summary>
public static class AccessBadgeMappingWorkflow
{
    public static AccessBadgeMappingManager NewAccessBadgeMappingManager(IAccessBadgeMappingStore store, IClock clock, Registry flags)
    {
        return new AccessBadgeMappingManager(
            store,
            AccessBadgeMappingRules.DefaultAccessBadgeMappingValidator(),
            clock,
            new List<string>(),
            AccessBadgeMappingEngine.DefaultAccessBadgeMappingEngine(flags));
    }
}
