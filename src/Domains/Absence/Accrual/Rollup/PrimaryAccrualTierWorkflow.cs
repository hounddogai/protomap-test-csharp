namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary accrual tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryAccrualTierDesk(IPrimaryAccrualTierStore Store, PrimaryAccrualTierValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAccrualTierStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary accrual tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAccrualTierStatus.Expired);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of primary accrual tier workflow.</summary>
public static class PrimaryAccrualTierWorkflow
{
    public static PrimaryAccrualTierDesk NewPrimaryAccrualTierDesk(IPrimaryAccrualTierStore store, IClock clock)
    {
        return new PrimaryAccrualTierDesk(
            store,
            PrimaryAccrualTierRules.DefaultPrimaryAccrualTierValidator(),
            clock,
            new List<string>());
    }
}
