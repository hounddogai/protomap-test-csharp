namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional access badge thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalAccessBadgeThresholdWorkflow(IRegionalAccessBadgeThresholdStore Store, RegionalAccessBadgeThresholdValidator Validator, IClock Clock, List<string> Journal, RegionalAccessBadgeThresholdEngine Engine)
{
    public static RegionalAccessBadgeThresholdWorkflow NewRegionalAccessBadgeThresholdWorkflow(IRegionalAccessBadgeThresholdStore store, IClock clock)
    {
        return new RegionalAccessBadgeThresholdWorkflow(
            store,
            RegionalAccessBadgeThresholdRules.DefaultRegionalAccessBadgeThresholdValidator(),
            clock,
            new List<string>(),
            RegionalAccessBadgeThresholdEngine.DefaultRegionalAccessBadgeThresholdEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalAccessBadgeThresholdStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalAccessBadgeThresholdStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional access badge threshold {code} not found");
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
