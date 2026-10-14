namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves bonus pool baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolBaselineManager(IBonusPoolBaselineStore Store, BonusPoolBaselineValidator Validator, IClock Clock, List<string> Journal, BonusPoolBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool baseline {code} not found");
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

/// <summary>Functions of bonus pool baseline workflow.</summary>
public static class BonusPoolBaselineWorkflow
{
    public static BonusPoolBaselineManager NewBonusPoolBaselineManager(IBonusPoolBaselineStore store, IClock clock)
    {
        return new BonusPoolBaselineManager(
            store,
            BonusPoolBaselineRules.DefaultBonusPoolBaselineValidator(),
            clock,
            new List<string>(),
            BonusPoolBaselineEngine.DefaultBonusPoolBaselineEngine());
    }
}
