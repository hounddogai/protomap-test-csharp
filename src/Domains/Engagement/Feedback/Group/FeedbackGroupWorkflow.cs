namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves feedback groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record FeedbackGroupCoordinator(IFeedbackGroupStore Store, FeedbackGroupValidator Validator, IClock Clock, List<string> Journal, FeedbackGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FeedbackGroupStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FeedbackGroupStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"feedback group {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of feedback group workflow.</summary>
public static class FeedbackGroupWorkflow
{
    public static FeedbackGroupCoordinator NewFeedbackGroupCoordinator(IFeedbackGroupStore store, IClock clock)
    {
        return new FeedbackGroupCoordinator(
            store,
            FeedbackGroupRules.DefaultFeedbackGroupValidator(),
            clock,
            new List<string>(),
            FeedbackGroupEngine.DefaultFeedbackGroupEngine());
    }
}
