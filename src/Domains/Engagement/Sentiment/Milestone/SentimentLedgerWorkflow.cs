namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves sentiment ledgers through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentLedgerCoordinator(ISentimentLedgerStore Store, SentimentLedgerValidator Validator, IClock Clock, List<string> Journal, SentimentLedgerEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment ledger {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentLedgerStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment ledger {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentLedgerStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment ledger {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of sentiment ledger workflow.</summary>
public static class SentimentLedgerWorkflow
{
    public static SentimentLedgerCoordinator NewSentimentLedgerCoordinator(ISentimentLedgerStore store, IClock clock)
    {
        return new SentimentLedgerCoordinator(
            store,
            SentimentLedgerRules.DefaultSentimentLedgerValidator(),
            clock,
            new List<string>(),
            SentimentLedgerEngine.DefaultSentimentLedgerEngine());
    }
}
