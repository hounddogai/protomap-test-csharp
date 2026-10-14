namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves sentiment baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentBaselineWorkflow(ISentimentBaselineStore Store, SentimentBaselineValidator Validator, IClock Clock, List<string> Journal, SentimentBaselineEngine Engine)
{
    public static SentimentBaselineWorkflow NewSentimentBaselineWorkflow(ISentimentBaselineStore store, IClock clock, Registry flags)
    {
        return new SentimentBaselineWorkflow(
            store,
            SentimentBaselineRules.DefaultSentimentBaselineValidator(),
            clock,
            new List<string>(),
            SentimentBaselineEngine.DefaultSentimentBaselineEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment baseline {code} not found");
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
