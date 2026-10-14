namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentProfileWorkflow(ISentimentProfileStore Store, SentimentProfileValidator Validator, IClock Clock, List<string> Journal, SentimentProfileEngine Engine)
{
    public static SentimentProfileWorkflow NewSentimentProfileWorkflow(ISentimentProfileStore store, IClock clock)
    {
        return new SentimentProfileWorkflow(
            store,
            SentimentProfileRules.DefaultSentimentProfileValidator(),
            clock,
            new List<string>(),
            SentimentProfileEngine.DefaultSentimentProfileEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentProfileStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentProfileStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment profile {code} not found");
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
