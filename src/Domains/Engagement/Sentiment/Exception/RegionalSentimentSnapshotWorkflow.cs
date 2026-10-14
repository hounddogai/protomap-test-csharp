namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional sentiment snapshots through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalSentimentSnapshotWorkflow(IRegionalSentimentSnapshotStore Store, RegionalSentimentSnapshotValidator Validator, IClock Clock, List<string> Journal, RegionalSentimentSnapshotEngine Engine)
{
    public static RegionalSentimentSnapshotWorkflow NewRegionalSentimentSnapshotWorkflow(IRegionalSentimentSnapshotStore store, IClock clock)
    {
        return new RegionalSentimentSnapshotWorkflow(
            store,
            RegionalSentimentSnapshotRules.DefaultRegionalSentimentSnapshotValidator(),
            clock,
            new List<string>(),
            RegionalSentimentSnapshotEngine.DefaultRegionalSentimentSnapshotEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional sentiment snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalSentimentSnapshotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional sentiment snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalSentimentSnapshotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional sentiment snapshot {code} not found");
        }
        var result = Engine.Evaluate(item);
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
