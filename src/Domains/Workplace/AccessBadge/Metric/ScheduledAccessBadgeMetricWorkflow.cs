namespace Acme.Workforce.Workplace.AccessBadge.Metric;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled access badge metrics through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledAccessBadgeMetricManager(IScheduledAccessBadgeMetricStore Store, ScheduledAccessBadgeMetricValidator Validator, IClock Clock, List<string> Journal, ScheduledAccessBadgeMetricEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled access badge metric {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledAccessBadgeMetricStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled access badge metric {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledAccessBadgeMetricStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled access badge metric {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of scheduled access badge metric workflow.</summary>
public static class ScheduledAccessBadgeMetricWorkflow
{
    public static ScheduledAccessBadgeMetricManager NewScheduledAccessBadgeMetricManager(IScheduledAccessBadgeMetricStore store, IClock clock)
    {
        return new ScheduledAccessBadgeMetricManager(
            store,
            ScheduledAccessBadgeMetricRules.DefaultScheduledAccessBadgeMetricValidator(),
            clock,
            new List<string>(),
            ScheduledAccessBadgeMetricEngine.DefaultScheduledAccessBadgeMetricEngine());
    }
}
