namespace Acme.Workforce.Compensation.Adjustment.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adjustment plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentPlanService(IAdjustmentPlanStore Store, AdjustmentPlanValidator Validator, IClock Clock, List<string> Journal, AdjustmentPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentPlanStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentPlanStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment plan {code} not found");
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

/// <summary>Functions of adjustment plan workflow.</summary>
public static class AdjustmentPlanWorkflow
{
    public static AdjustmentPlanService NewAdjustmentPlanService(IAdjustmentPlanStore store, IClock clock, Registry flags)
    {
        return new AdjustmentPlanService(
            store,
            AdjustmentPlanRules.DefaultAdjustmentPlanValidator(),
            clock,
            new List<string>(),
            AdjustmentPlanEngine.DefaultAdjustmentPlanEngine(flags));
    }
}
