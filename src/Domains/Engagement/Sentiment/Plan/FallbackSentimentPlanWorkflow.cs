namespace Acme.Workforce.Engagement.Sentiment.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback sentiment plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackSentimentPlanManager(IFallbackSentimentPlanStore Store, FallbackSentimentPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback sentiment plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackSentimentPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback sentiment plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackSentimentPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of fallback sentiment plan workflow.</summary>
public static class FallbackSentimentPlanWorkflow
{
    public static FallbackSentimentPlanManager NewFallbackSentimentPlanManager(IFallbackSentimentPlanStore store, IClock clock)
    {
        return new FallbackSentimentPlanManager(
            store,
            FallbackSentimentPlanRules.DefaultFallbackSentimentPlanValidator(),
            clock,
            new List<string>());
    }
}
