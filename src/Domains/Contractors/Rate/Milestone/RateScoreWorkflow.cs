namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rate scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record RateScoreService(IRateScoreStore Store, RateScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RateScoreStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RateScoreStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of rate score workflow.</summary>
public static class RateScoreWorkflow
{
    public static RateScoreService NewRateScoreService(IRateScoreStore store, IClock clock)
    {
        return new RateScoreService(store, RateScoreRules.DefaultRateScoreValidator(), clock, new List<string>());
    }
}
