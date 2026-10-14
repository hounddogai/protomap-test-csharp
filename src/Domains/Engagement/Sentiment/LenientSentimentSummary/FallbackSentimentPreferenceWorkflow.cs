namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback sentiment preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackSentimentPreferenceCoordinator(IFallbackSentimentPreferenceStore Store, FallbackSentimentPreferenceValidator Validator, IClock Clock, List<string> Journal, FallbackSentimentPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback sentiment preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackSentimentPreferenceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback sentiment preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackSentimentPreferenceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback sentiment preference {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of fallback sentiment preference workflow.</summary>
public static class FallbackSentimentPreferenceWorkflow
{
    public static FallbackSentimentPreferenceCoordinator NewFallbackSentimentPreferenceCoordinator(IFallbackSentimentPreferenceStore store, IClock clock)
    {
        return new FallbackSentimentPreferenceCoordinator(
            store,
            FallbackSentimentPreferenceRules.DefaultFallbackSentimentPreferenceValidator(),
            clock,
            new List<string>(),
            FallbackSentimentPreferenceEngine.DefaultFallbackSentimentPreferenceEngine());
    }
}
