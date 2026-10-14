namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending pay band tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPayBandTierManager(IPendingPayBandTierStore Store, PendingPayBandTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPayBandTierStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending pay band tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPayBandTierStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of pending pay band tier workflow.</summary>
public static class PendingPayBandTierWorkflow
{
    public static PendingPayBandTierManager NewPendingPayBandTierManager(IPendingPayBandTierStore store, IClock clock)
    {
        return new PendingPayBandTierManager(
            store,
            PendingPayBandTierRules.DefaultPendingPayBandTierValidator(),
            clock,
            new List<string>());
    }
}
