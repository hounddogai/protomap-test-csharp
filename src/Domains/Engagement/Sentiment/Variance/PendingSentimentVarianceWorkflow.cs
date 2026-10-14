namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending sentiment variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingSentimentVarianceManager(IPendingSentimentVarianceStore Store, PendingSentimentVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending sentiment variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingSentimentVarianceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending sentiment variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingSentimentVarianceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of pending sentiment variance workflow.</summary>
public static class PendingSentimentVarianceWorkflow
{
    public static PendingSentimentVarianceManager NewPendingSentimentVarianceManager(IPendingSentimentVarianceStore store, IClock clock)
    {
        return new PendingSentimentVarianceManager(
            store,
            PendingSentimentVarianceRules.DefaultPendingSentimentVarianceValidator(),
            clock,
            new List<string>());
    }
}
