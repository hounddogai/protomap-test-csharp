namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayTierDesk(IRetroPayTierStore Store, RetroPayTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayTierStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayTierStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of retro pay tier workflow.</summary>
public static class RetroPayTierWorkflow
{
    public static RetroPayTierDesk NewRetroPayTierDesk(IRetroPayTierStore store, IClock clock)
    {
        return new RetroPayTierDesk(
            store,
            RetroPayTierRules.DefaultRetroPayTierValidator(),
            clock,
            new List<string>());
    }
}
