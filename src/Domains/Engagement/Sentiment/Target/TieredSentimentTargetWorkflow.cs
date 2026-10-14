namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered sentiment targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredSentimentTargetManager(ITieredSentimentTargetStore Store, TieredSentimentTargetValidator Validator, IClock Clock, List<string> Journal, TieredSentimentTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered sentiment target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredSentimentTargetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered sentiment target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredSentimentTargetStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered sentiment target {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of tiered sentiment target workflow.</summary>
public static class TieredSentimentTargetWorkflow
{
    public static TieredSentimentTargetManager NewTieredSentimentTargetManager(ITieredSentimentTargetStore store, IClock clock, Registry flags)
    {
        return new TieredSentimentTargetManager(
            store,
            TieredSentimentTargetRules.DefaultTieredSentimentTargetValidator(),
            clock,
            new List<string>(),
            TieredSentimentTargetEngine.DefaultTieredSentimentTargetEngine(flags));
    }
}
