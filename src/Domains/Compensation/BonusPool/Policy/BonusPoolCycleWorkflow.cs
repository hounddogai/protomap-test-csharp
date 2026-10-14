namespace Acme.Workforce.Compensation.BonusPool.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolCycleService(IBonusPoolCycleStore Store, BonusPoolCycleValidator Validator, IClock Clock, List<string> Journal, BonusPoolCycleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolCycleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolCycleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool cycle {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of bonus pool cycle workflow.</summary>
public static class BonusPoolCycleWorkflow
{
    public static BonusPoolCycleService NewBonusPoolCycleService(IBonusPoolCycleStore store, IClock clock, Registry flags)
    {
        return new BonusPoolCycleService(
            store,
            BonusPoolCycleRules.DefaultBonusPoolCycleValidator(),
            clock,
            new List<string>(),
            BonusPoolCycleEngine.DefaultBonusPoolCycleEngine(flags));
    }
}
