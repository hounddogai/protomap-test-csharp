namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient sentiment quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientSentimentQuotaDesk(ILenientSentimentQuotaStore Store, LenientSentimentQuotaValidator Validator, IClock Clock, List<string> Journal, LenientSentimentQuotaEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientSentimentQuotaStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientSentimentQuotaStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment quota {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of lenient sentiment quota workflow.</summary>
public static class LenientSentimentQuotaWorkflow
{
    public static LenientSentimentQuotaDesk NewLenientSentimentQuotaDesk(ILenientSentimentQuotaStore store, IClock clock, Registry flags)
    {
        return new LenientSentimentQuotaDesk(
            store,
            LenientSentimentQuotaRules.DefaultLenientSentimentQuotaValidator(),
            clock,
            new List<string>(),
            LenientSentimentQuotaEngine.DefaultLenientSentimentQuotaEngine(flags));
    }
}
