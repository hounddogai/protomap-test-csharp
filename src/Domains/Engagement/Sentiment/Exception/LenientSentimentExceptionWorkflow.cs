namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient sentiment exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientSentimentExceptionManager(ILenientSentimentExceptionStore Store, LenientSentimentExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientSentimentExceptionStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient sentiment exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientSentimentExceptionStatus.Failed);
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

/// <summary>Functions of lenient sentiment exception workflow.</summary>
public static class LenientSentimentExceptionWorkflow
{
    public static LenientSentimentExceptionManager NewLenientSentimentExceptionManager(ILenientSentimentExceptionStore store, IClock clock)
    {
        return new LenientSentimentExceptionManager(
            store,
            LenientSentimentExceptionRules.DefaultLenientSentimentExceptionValidator(),
            clock,
            new List<string>());
    }
}
