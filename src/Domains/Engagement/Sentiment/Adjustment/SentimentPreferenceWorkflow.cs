namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentPreferenceCoordinator(ISentimentPreferenceStore Store, SentimentPreferenceValidator Validator, IClock Clock, List<string> Journal, SentimentPreferenceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentPreferenceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentPreferenceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment preference {code} not found");
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

/// <summary>Functions of sentiment preference workflow.</summary>
public static class SentimentPreferenceWorkflow
{
    public static SentimentPreferenceCoordinator NewSentimentPreferenceCoordinator(ISentimentPreferenceStore store, IClock clock)
    {
        return new SentimentPreferenceCoordinator(
            store,
            SentimentPreferenceRules.DefaultSentimentPreferenceValidator(),
            clock,
            new List<string>(),
            SentimentPreferenceEngine.DefaultSentimentPreferenceEngine());
    }
}
