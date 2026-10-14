namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves feedback snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackSnapshotCoordinator(IFeedbackSnapshotStore Store, FeedbackSnapshotValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackSnapshotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackSnapshotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of feedback snapshot workflow.</summary>
public static class FeedbackSnapshotWorkflow
{
    public static FeedbackSnapshotCoordinator NewFeedbackSnapshotCoordinator(IFeedbackSnapshotStore store, IClock clock)
    {
        return new FeedbackSnapshotCoordinator(
            store,
            FeedbackSnapshotRules.DefaultFeedbackSnapshotValidator(),
            clock,
            new List<string>());
    }
}
