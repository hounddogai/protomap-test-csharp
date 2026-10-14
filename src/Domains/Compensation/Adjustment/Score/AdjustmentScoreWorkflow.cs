namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adjustment scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentScoreService(IAdjustmentScoreStore Store, AdjustmentScoreValidator Validator, IClock Clock, List<string> Journal, AdjustmentScoreEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentScoreStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentScoreStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment score {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of adjustment score workflow.</summary>
public static class AdjustmentScoreWorkflow
{
    public static AdjustmentScoreService NewAdjustmentScoreService(IAdjustmentScoreStore store, IClock clock, Registry flags)
    {
        return new AdjustmentScoreService(
            store,
            AdjustmentScoreRules.DefaultAdjustmentScoreValidator(),
            clock,
            new List<string>(),
            AdjustmentScoreEngine.DefaultAdjustmentScoreEngine(flags));
    }
}
