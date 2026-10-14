namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled sentiment targets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledSentimentTargetCoordinator(IScheduledSentimentTargetStore Store, ScheduledSentimentTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled sentiment target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledSentimentTargetStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled sentiment target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledSentimentTargetStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of scheduled sentiment target workflow.</summary>
public static class ScheduledSentimentTargetWorkflow
{
    public static ScheduledSentimentTargetCoordinator NewScheduledSentimentTargetCoordinator(IScheduledSentimentTargetStore store, IClock clock)
    {
        return new ScheduledSentimentTargetCoordinator(
            store,
            ScheduledSentimentTargetRules.DefaultScheduledSentimentTargetValidator(),
            clock,
            new List<string>());
    }
}
