namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict engagement snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictEngagementSnapshotWorkflow(IStrictEngagementSnapshotStore Store, StrictEngagementSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictEngagementSnapshotWorkflow NewStrictEngagementSnapshotWorkflow(IStrictEngagementSnapshotStore store, IClock clock)
    {
        return new StrictEngagementSnapshotWorkflow(
            store,
            StrictEngagementSnapshotRules.DefaultStrictEngagementSnapshotValidator(),
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
            throw new InvalidOperationException($"strict engagement snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictEngagementSnapshotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict engagement snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictEngagementSnapshotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
