namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled nomination snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledNominationSnapshotService(IScheduledNominationSnapshotStore Store, ScheduledNominationSnapshotValidator Validator, IClock Clock, List<string> Journal, ScheduledNominationSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled nomination snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledNominationSnapshotStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled nomination snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledNominationSnapshotStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled nomination snapshot {code} not found");
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

/// <summary>Functions of scheduled nomination snapshot workflow.</summary>
public static class ScheduledNominationSnapshotWorkflow
{
    public static ScheduledNominationSnapshotService NewScheduledNominationSnapshotService(IScheduledNominationSnapshotStore store, IClock clock)
    {
        return new ScheduledNominationSnapshotService(
            store,
            ScheduledNominationSnapshotRules.DefaultScheduledNominationSnapshotValidator(),
            clock,
            new List<string>(),
            ScheduledNominationSnapshotEngine.DefaultScheduledNominationSnapshotEngine());
    }
}
