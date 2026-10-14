namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolBundleCoordinator(IBonusPoolBundleStore Store, BonusPoolBundleValidator Validator, IClock Clock, List<string> Journal, BonusPoolBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolBundleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolBundleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool bundle {code} not found");
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

/// <summary>Functions of bonus pool bundle workflow.</summary>
public static class BonusPoolBundleWorkflow
{
    public static BonusPoolBundleCoordinator NewBonusPoolBundleCoordinator(IBonusPoolBundleStore store, IClock clock, Registry flags)
    {
        return new BonusPoolBundleCoordinator(
            store,
            BonusPoolBundleRules.DefaultBonusPoolBundleValidator(),
            clock,
            new List<string>(),
            BonusPoolBundleEngine.DefaultBonusPoolBundleEngine(flags));
    }
}
