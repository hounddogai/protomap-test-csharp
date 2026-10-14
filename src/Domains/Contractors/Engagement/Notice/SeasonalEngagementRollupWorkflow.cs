namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal engagement rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalEngagementRollupDesk(ISeasonalEngagementRollupStore Store, SeasonalEngagementRollupValidator Validator, IClock Clock, List<string> Journal, SeasonalEngagementRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalEngagementRollupStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalEngagementRollupStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal engagement rollup {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of seasonal engagement rollup workflow.</summary>
public static class SeasonalEngagementRollupWorkflow
{
    public static SeasonalEngagementRollupDesk NewSeasonalEngagementRollupDesk(ISeasonalEngagementRollupStore store, IClock clock, Registry flags)
    {
        return new SeasonalEngagementRollupDesk(
            store,
            SeasonalEngagementRollupRules.DefaultSeasonalEngagementRollupValidator(),
            clock,
            new List<string>(),
            SeasonalEngagementRollupEngine.DefaultSeasonalEngagementRollupEngine(flags));
    }
}
