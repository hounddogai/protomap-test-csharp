namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback pay band scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackPayBandScoreManager(IFallbackPayBandScoreStore Store, FallbackPayBandScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPayBandScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback pay band score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPayBandScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of fallback pay band score workflow.</summary>
public static class FallbackPayBandScoreWorkflow
{
    public static FallbackPayBandScoreManager NewFallbackPayBandScoreManager(IFallbackPayBandScoreStore store, IClock clock)
    {
        return new FallbackPayBandScoreManager(
            store,
            FallbackPayBandScoreRules.DefaultFallbackPayBandScoreValidator(),
            clock,
            new List<string>());
    }
}
