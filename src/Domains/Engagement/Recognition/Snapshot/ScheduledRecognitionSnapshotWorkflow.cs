namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled recognition snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledRecognitionSnapshotDesk(IScheduledRecognitionSnapshotStore Store, ScheduledRecognitionSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled recognition snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledRecognitionSnapshotStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled recognition snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledRecognitionSnapshotStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of scheduled recognition snapshot workflow.</summary>
public static class ScheduledRecognitionSnapshotWorkflow
{
    public static ScheduledRecognitionSnapshotDesk NewScheduledRecognitionSnapshotDesk(IScheduledRecognitionSnapshotStore store, IClock clock)
    {
        return new ScheduledRecognitionSnapshotDesk(
            store,
            ScheduledRecognitionSnapshotRules.DefaultScheduledRecognitionSnapshotValidator(),
            clock,
            new List<string>());
    }
}
