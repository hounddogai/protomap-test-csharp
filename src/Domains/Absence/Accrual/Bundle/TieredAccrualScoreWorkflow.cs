namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered accrual scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredAccrualScoreManager(ITieredAccrualScoreStore Store, TieredAccrualScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAccrualScoreStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered accrual score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAccrualScoreStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of tiered accrual score workflow.</summary>
public static class TieredAccrualScoreWorkflow
{
    public static TieredAccrualScoreManager NewTieredAccrualScoreManager(ITieredAccrualScoreStore store, IClock clock)
    {
        return new TieredAccrualScoreManager(
            store,
            TieredAccrualScoreRules.DefaultTieredAccrualScoreValidator(),
            clock,
            new List<string>());
    }
}
