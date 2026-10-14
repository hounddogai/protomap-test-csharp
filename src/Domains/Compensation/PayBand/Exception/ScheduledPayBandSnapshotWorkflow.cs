namespace Acme.Workforce.Compensation.PayBand.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled pay band snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledPayBandSnapshotCoordinator(IScheduledPayBandSnapshotStore Store, ScheduledPayBandSnapshotValidator Validator, IClock Clock, List<string> Journal, ScheduledPayBandSnapshotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledPayBandSnapshotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledPayBandSnapshotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled pay band snapshot {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of scheduled pay band snapshot workflow.</summary>
public static class ScheduledPayBandSnapshotWorkflow
{
    public static ScheduledPayBandSnapshotCoordinator NewScheduledPayBandSnapshotCoordinator(IScheduledPayBandSnapshotStore store, IClock clock)
    {
        return new ScheduledPayBandSnapshotCoordinator(
            store,
            ScheduledPayBandSnapshotRules.DefaultScheduledPayBandSnapshotValidator(),
            clock,
            new List<string>(),
            ScheduledPayBandSnapshotEngine.DefaultScheduledPayBandSnapshotEngine());
    }
}
