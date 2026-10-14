namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched sentiment manifests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record BatchedSentimentManifestManager(IBatchedSentimentManifestStore Store, BatchedSentimentManifestValidator Validator, IClock Clock, List<string> Journal, BatchedSentimentManifestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched sentiment manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedSentimentManifestStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched sentiment manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedSentimentManifestStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched sentiment manifest {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of batched sentiment manifest workflow.</summary>
public static class BatchedSentimentManifestWorkflow
{
    public static BatchedSentimentManifestManager NewBatchedSentimentManifestManager(IBatchedSentimentManifestStore store, IClock clock, Registry flags)
    {
        return new BatchedSentimentManifestManager(
            store,
            BatchedSentimentManifestRules.DefaultBatchedSentimentManifestValidator(),
            clock,
            new List<string>(),
            BatchedSentimentManifestEngine.DefaultBatchedSentimentManifestEngine(flags));
    }
}
