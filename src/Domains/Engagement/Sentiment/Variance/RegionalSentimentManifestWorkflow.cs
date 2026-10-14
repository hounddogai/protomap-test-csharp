namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional sentiment manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalSentimentManifestManager(IRegionalSentimentManifestStore Store, RegionalSentimentManifestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional sentiment manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSentimentManifestStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional sentiment manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSentimentManifestStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of regional sentiment manifest workflow.</summary>
public static class RegionalSentimentManifestWorkflow
{
    public static RegionalSentimentManifestManager NewRegionalSentimentManifestManager(IRegionalSentimentManifestStore store, IClock clock)
    {
        return new RegionalSentimentManifestManager(
            store,
            RegionalSentimentManifestRules.DefaultRegionalSentimentManifestValidator(),
            clock,
            new List<string>());
    }
}
