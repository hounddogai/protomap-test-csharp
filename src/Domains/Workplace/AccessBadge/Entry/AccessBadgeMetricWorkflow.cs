namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves access badge metrics through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeMetricManager(IAccessBadgeMetricStore Store, AccessBadgeMetricValidator Validator, IClock Clock, List<string> Journal, AccessBadgeMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeMetricStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeMetricStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge metric {code} not found");
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

/// <summary>Functions of access badge metric workflow.</summary>
public static class AccessBadgeMetricWorkflow
{
    public static AccessBadgeMetricManager NewAccessBadgeMetricManager(IAccessBadgeMetricStore store, IClock clock, Registry flags)
    {
        return new AccessBadgeMetricManager(
            store,
            AccessBadgeMetricRules.DefaultAccessBadgeMetricValidator(),
            clock,
            new List<string>(),
            AccessBadgeMetricEngine.DefaultAccessBadgeMetricEngine(flags));
    }
}
