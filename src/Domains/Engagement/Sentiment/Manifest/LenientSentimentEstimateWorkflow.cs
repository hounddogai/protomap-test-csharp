namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient sentiment estimates through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientSentimentEstimateService(ILenientSentimentEstimateStore Store, LenientSentimentEstimateValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientSentimentEstimateStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientSentimentEstimateStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of lenient sentiment estimate workflow.</summary>
public static class LenientSentimentEstimateWorkflow
{
    public static LenientSentimentEstimateService NewLenientSentimentEstimateService(ILenientSentimentEstimateStore store, IClock clock)
    {
        return new LenientSentimentEstimateService(
            store,
            LenientSentimentEstimateRules.DefaultLenientSentimentEstimateValidator(),
            clock,
            new List<string>());
    }
}
