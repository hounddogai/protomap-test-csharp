namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict sentiment quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictSentimentQuotaManager(IStrictSentimentQuotaStore Store, StrictSentimentQuotaValidator Validator, IClock Clock, List<string> Journal, StrictSentimentQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict sentiment quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictSentimentQuotaStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict sentiment quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictSentimentQuotaStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict sentiment quota {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of strict sentiment quota workflow.</summary>
public static class StrictSentimentQuotaWorkflow
{
    public static StrictSentimentQuotaManager NewStrictSentimentQuotaManager(IStrictSentimentQuotaStore store, IClock clock)
    {
        return new StrictSentimentQuotaManager(
            store,
            StrictSentimentQuotaRules.DefaultStrictSentimentQuotaValidator(),
            clock,
            new List<string>(),
            StrictSentimentQuotaEngine.DefaultStrictSentimentQuotaEngine());
    }
}
