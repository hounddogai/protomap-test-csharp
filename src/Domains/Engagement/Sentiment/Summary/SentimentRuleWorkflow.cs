namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentRuleDesk(ISentimentRuleStore Store, SentimentRuleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentRuleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentRuleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of sentiment rule workflow.</summary>
public static class SentimentRuleWorkflow
{
    public static SentimentRuleDesk NewSentimentRuleDesk(ISentimentRuleStore store, IClock clock)
    {
        return new SentimentRuleDesk(
            store,
            SentimentRuleRules.DefaultSentimentRuleValidator(),
            clock,
            new List<string>());
    }
}
