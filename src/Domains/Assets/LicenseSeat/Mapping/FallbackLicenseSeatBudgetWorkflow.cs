namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback license seat budgets through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackLicenseSeatBudgetDesk(IFallbackLicenseSeatBudgetStore Store, FallbackLicenseSeatBudgetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat budget {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackLicenseSeatBudgetStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback license seat budget {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackLicenseSeatBudgetStatus.Expired);
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

/// <summary>Functions of fallback license seat budget workflow.</summary>
public static class FallbackLicenseSeatBudgetWorkflow
{
    public static FallbackLicenseSeatBudgetDesk NewFallbackLicenseSeatBudgetDesk(IFallbackLicenseSeatBudgetStore store, IClock clock)
    {
        return new FallbackLicenseSeatBudgetDesk(
            store,
            FallbackLicenseSeatBudgetRules.DefaultFallbackLicenseSeatBudgetValidator(),
            clock,
            new List<string>());
    }
}
