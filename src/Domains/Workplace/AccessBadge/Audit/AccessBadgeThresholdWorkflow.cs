namespace Acme.Workforce.Workplace.AccessBadge.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeThresholdService(IAccessBadgeThresholdStore Store, AccessBadgeThresholdValidator Validator, IClock Clock, List<string> Journal, AccessBadgeThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeThresholdStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeThresholdStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge threshold {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of access badge threshold workflow.</summary>
public static class AccessBadgeThresholdWorkflow
{
    public static AccessBadgeThresholdService NewAccessBadgeThresholdService(IAccessBadgeThresholdStore store, IClock clock)
    {
        return new AccessBadgeThresholdService(
            store,
            AccessBadgeThresholdRules.DefaultAccessBadgeThresholdValidator(),
            clock,
            new List<string>(),
            AccessBadgeThresholdEngine.DefaultAccessBadgeThresholdEngine());
    }
}
