namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adaptive engagement notices through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveEngagementNoticeManager(IAdaptiveEngagementNoticeStore Store, AdaptiveEngagementNoticeValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive engagement notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveEngagementNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive engagement notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveEngagementNoticeStatus.Failed);
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

/// <summary>Functions of adaptive engagement notice workflow.</summary>
public static class AdaptiveEngagementNoticeWorkflow
{
    public static AdaptiveEngagementNoticeManager NewAdaptiveEngagementNoticeManager(IAdaptiveEngagementNoticeStore store, IClock clock)
    {
        return new AdaptiveEngagementNoticeManager(
            store,
            AdaptiveEngagementNoticeRules.DefaultAdaptiveEngagementNoticeValidator(),
            clock,
            new List<string>());
    }
}
