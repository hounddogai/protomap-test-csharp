namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentThresholdDesk(ISentimentThresholdStore Store, SentimentThresholdValidator Validator, IClock Clock, List<string> Journal, SentimentThresholdEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentThresholdStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentThresholdStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment threshold {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of sentiment threshold workflow.</summary>
public static class SentimentThresholdWorkflow
{
    public static SentimentThresholdDesk NewSentimentThresholdDesk(ISentimentThresholdStore store, IClock clock)
    {
        return new SentimentThresholdDesk(
            store,
            SentimentThresholdRules.DefaultSentimentThresholdValidator(),
            clock,
            new List<string>(),
            SentimentThresholdEngine.DefaultSentimentThresholdEngine());
    }
}
