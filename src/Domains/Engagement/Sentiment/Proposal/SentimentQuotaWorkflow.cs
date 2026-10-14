namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentQuotaManager(ISentimentQuotaStore Store, SentimentQuotaValidator Validator, IClock Clock, List<string> Journal, SentimentQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentQuotaStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentQuotaStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment quota {code} not found");
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

/// <summary>Functions of sentiment quota workflow.</summary>
public static class SentimentQuotaWorkflow
{
    public static SentimentQuotaManager NewSentimentQuotaManager(ISentimentQuotaStore store, IClock clock)
    {
        return new SentimentQuotaManager(
            store,
            SentimentQuotaRules.DefaultSentimentQuotaValidator(),
            clock,
            new List<string>(),
            SentimentQuotaEngine.DefaultSentimentQuotaEngine());
    }
}
