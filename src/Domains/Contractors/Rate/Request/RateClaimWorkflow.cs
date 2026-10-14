namespace Acme.Workforce.Contractors.Rate.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rate claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record RateClaimCoordinator(IRateClaimStore Store, RateClaimValidator Validator, IClock Clock, List<string> Journal, RateClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RateClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RateClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate claim {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of rate claim workflow.</summary>
public static class RateClaimWorkflow
{
    public static RateClaimCoordinator NewRateClaimCoordinator(IRateClaimStore store, IClock clock)
    {
        return new RateClaimCoordinator(
            store,
            RateClaimRules.DefaultRateClaimValidator(),
            clock,
            new List<string>(),
            RateClaimEngine.DefaultRateClaimEngine());
    }
}
