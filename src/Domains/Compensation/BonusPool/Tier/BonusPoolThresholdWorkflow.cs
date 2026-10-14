namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool thresholds through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolThresholdWorkflow(IBonusPoolThresholdStore Store, BonusPoolThresholdValidator Validator, IClock Clock, List<string> Journal, BonusPoolThresholdEngine Engine)
{
    public static BonusPoolThresholdWorkflow NewBonusPoolThresholdWorkflow(IBonusPoolThresholdStore store, IClock clock, Registry flags)
    {
        return new BonusPoolThresholdWorkflow(
            store,
            BonusPoolThresholdRules.DefaultBonusPoolThresholdValidator(),
            clock,
            new List<string>(),
            BonusPoolThresholdEngine.DefaultBonusPoolThresholdEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolThresholdStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolThresholdStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool threshold {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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
