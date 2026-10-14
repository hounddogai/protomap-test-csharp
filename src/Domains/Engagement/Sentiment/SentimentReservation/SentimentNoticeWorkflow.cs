namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentNoticeCoordinator(ISentimentNoticeStore Store, SentimentNoticeValidator Validator, IClock Clock, List<string> Journal, SentimentNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentNoticeStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentNoticeStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment notice {code} not found");
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

/// <summary>Functions of sentiment notice workflow.</summary>
public static class SentimentNoticeWorkflow
{
    public static SentimentNoticeCoordinator NewSentimentNoticeCoordinator(ISentimentNoticeStore store, IClock clock)
    {
        return new SentimentNoticeCoordinator(
            store,
            SentimentNoticeRules.DefaultSentimentNoticeValidator(),
            clock,
            new List<string>(),
            SentimentNoticeEngine.DefaultSentimentNoticeEngine());
    }
}
