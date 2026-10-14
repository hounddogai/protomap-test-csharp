namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentManifestDesk(ISentimentManifestStore Store, SentimentManifestValidator Validator, IClock Clock, List<string> Journal, SentimentManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentManifestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentManifestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment manifest {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of sentiment manifest workflow.</summary>
public static class SentimentManifestWorkflow
{
    public static SentimentManifestDesk NewSentimentManifestDesk(ISentimentManifestStore store, IClock clock)
    {
        return new SentimentManifestDesk(
            store,
            SentimentManifestRules.DefaultSentimentManifestValidator(),
            clock,
            new List<string>(),
            SentimentManifestEngine.DefaultSentimentManifestEngine());
    }
}
