namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves accrual rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccrualRollupService(IAccrualRollupStore Store, AccrualRollupValidator Validator, IClock Clock, List<string> Journal, AccrualRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccrualRollupStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccrualRollupStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"accrual rollup {code} not found");
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

/// <summary>Functions of accrual rollup workflow.</summary>
public static class AccrualRollupWorkflow
{
    public static AccrualRollupService NewAccrualRollupService(IAccrualRollupStore store, IClock clock, Registry flags)
    {
        return new AccrualRollupService(
            store,
            AccrualRollupRules.DefaultAccrualRollupValidator(),
            clock,
            new List<string>(),
            AccrualRollupEngine.DefaultAccrualRollupEngine(flags));
    }
}
