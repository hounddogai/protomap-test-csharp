namespace Acme.Workforce.Absence.Accrual.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves accrual scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualScoreService(IAccrualScoreStore Store, AccrualScoreValidator Validator, IClock Clock, List<string> Journal, AccrualScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualScoreStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualScoreStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual score {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of accrual score workflow.</summary>
public static class AccrualScoreWorkflow
{
    public static AccrualScoreService NewAccrualScoreService(IAccrualScoreStore store, IClock clock, Registry flags)
    {
        return new AccrualScoreService(
            store,
            AccrualScoreRules.DefaultAccrualScoreValidator(),
            clock,
            new List<string>(),
            AccrualScoreEngine.DefaultAccrualScoreEngine(flags));
    }
}
