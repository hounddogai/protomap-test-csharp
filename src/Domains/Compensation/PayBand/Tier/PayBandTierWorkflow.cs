namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pay band tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandTierCoordinator(IPayBandTierStore Store, PayBandTierValidator Validator, IClock Clock, List<string> Journal, PayBandTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandTierStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandTierStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band tier {code} not found");
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

/// <summary>Functions of pay band tier workflow.</summary>
public static class PayBandTierWorkflow
{
    public static PayBandTierCoordinator NewPayBandTierCoordinator(IPayBandTierStore store, IClock clock)
    {
        return new PayBandTierCoordinator(
            store,
            PayBandTierRules.DefaultPayBandTierValidator(),
            clock,
            new List<string>(),
            PayBandTierEngine.DefaultPayBandTierEngine());
    }
}
