namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global booking tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalBookingTierService(IGlobalBookingTierStore Store, GlobalBookingTierValidator Validator, IClock Clock, List<string> Journal, GlobalBookingTierEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global booking tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalBookingTierStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global booking tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalBookingTierStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global booking tier {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of global booking tier workflow.</summary>
public static class GlobalBookingTierWorkflow
{
    public static GlobalBookingTierService NewGlobalBookingTierService(IGlobalBookingTierStore store, IClock clock)
    {
        return new GlobalBookingTierService(
            store,
            GlobalBookingTierRules.DefaultGlobalBookingTierValidator(),
            clock,
            new List<string>(),
            GlobalBookingTierEngine.DefaultGlobalBookingTierEngine());
    }
}
