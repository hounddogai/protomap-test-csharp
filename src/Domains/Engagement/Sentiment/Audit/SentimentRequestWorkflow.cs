namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentRequestCoordinator(ISentimentRequestStore Store, SentimentRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentRequestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentRequestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of sentiment request workflow.</summary>
public static class SentimentRequestWorkflow
{
    public static SentimentRequestCoordinator NewSentimentRequestCoordinator(ISentimentRequestStore store, IClock clock)
    {
        return new SentimentRequestCoordinator(
            store,
            SentimentRequestRules.DefaultSentimentRequestValidator(),
            clock,
            new List<string>());
    }
}
