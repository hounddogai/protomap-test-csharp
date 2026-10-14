namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered pay band scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPayBandScoreDesk(ITieredPayBandScoreStore Store, TieredPayBandScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pay band score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPayBandScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered pay band score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPayBandScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of tiered pay band score workflow.</summary>
public static class TieredPayBandScoreWorkflow
{
    public static TieredPayBandScoreDesk NewTieredPayBandScoreDesk(ITieredPayBandScoreStore store, IClock clock)
    {
        return new TieredPayBandScoreDesk(
            store,
            TieredPayBandScoreRules.DefaultTieredPayBandScoreValidator(),
            clock,
            new List<string>());
    }
}
