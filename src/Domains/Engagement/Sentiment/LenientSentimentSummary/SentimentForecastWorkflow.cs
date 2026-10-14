namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment forecasts through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentForecastService(ISentimentForecastStore Store, SentimentForecastValidator Validator, IClock Clock, List<string> Journal, SentimentForecastEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment forecast {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentForecastStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment forecast {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentForecastStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment forecast {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of sentiment forecast workflow.</summary>
public static class SentimentForecastWorkflow
{
    public static SentimentForecastService NewSentimentForecastService(ISentimentForecastStore store, IClock clock)
    {
        return new SentimentForecastService(
            store,
            SentimentForecastRules.DefaultSentimentForecastValidator(),
            clock,
            new List<string>(),
            SentimentForecastEngine.DefaultSentimentForecastEngine());
    }
}
