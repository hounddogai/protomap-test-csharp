namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentAssignmentCoordinator(ISentimentAssignmentStore Store, SentimentAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentAssignmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentAssignmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of sentiment assignment workflow.</summary>
public static class SentimentAssignmentWorkflow
{
    public static SentimentAssignmentCoordinator NewSentimentAssignmentCoordinator(ISentimentAssignmentStore store, IClock clock)
    {
        return new SentimentAssignmentCoordinator(
            store,
            SentimentAssignmentRules.DefaultSentimentAssignmentValidator(),
            clock,
            new List<string>());
    }
}
