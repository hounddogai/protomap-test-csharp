namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled mobility rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledMobilityRollupWorkflow(IScheduledMobilityRollupStore Store, ScheduledMobilityRollupValidator Validator, IClock Clock, List<string> Journal)
{
    public static ScheduledMobilityRollupWorkflow NewScheduledMobilityRollupWorkflow(IScheduledMobilityRollupStore store, IClock clock)
    {
        return new ScheduledMobilityRollupWorkflow(
            store,
            ScheduledMobilityRollupRules.DefaultScheduledMobilityRollupValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledMobilityRollupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled mobility rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledMobilityRollupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
