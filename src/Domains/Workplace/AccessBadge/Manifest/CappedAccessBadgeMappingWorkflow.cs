namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped access badge mappings through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedAccessBadgeMappingCoordinator(ICappedAccessBadgeMappingStore Store, CappedAccessBadgeMappingValidator Validator, IClock Clock, List<string> Journal, CappedAccessBadgeMappingEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge mapping {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAccessBadgeMappingStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge mapping {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAccessBadgeMappingStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped access badge mapping {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of capped access badge mapping workflow.</summary>
public static class CappedAccessBadgeMappingWorkflow
{
    public static CappedAccessBadgeMappingCoordinator NewCappedAccessBadgeMappingCoordinator(ICappedAccessBadgeMappingStore store, IClock clock)
    {
        return new CappedAccessBadgeMappingCoordinator(
            store,
            CappedAccessBadgeMappingRules.DefaultCappedAccessBadgeMappingValidator(),
            clock,
            new List<string>(),
            CappedAccessBadgeMappingEngine.DefaultCappedAccessBadgeMappingEngine());
    }
}
