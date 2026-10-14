namespace Acme.Workforce.Workplace.AccessBadge.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgePlanCoordinator(IAccessBadgePlanStore Store, AccessBadgePlanValidator Validator, IClock Clock, List<string> Journal, AccessBadgePlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgePlanStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgePlanStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge plan {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of access badge plan workflow.</summary>
public static class AccessBadgePlanWorkflow
{
    public static AccessBadgePlanCoordinator NewAccessBadgePlanCoordinator(IAccessBadgePlanStore store, IClock clock)
    {
        return new AccessBadgePlanCoordinator(
            store,
            AccessBadgePlanRules.DefaultAccessBadgePlanValidator(),
            clock,
            new List<string>(),
            AccessBadgePlanEngine.DefaultAccessBadgePlanEngine());
    }
}
