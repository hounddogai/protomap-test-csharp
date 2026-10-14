namespace Acme.Workforce.Compensation.Adjustment.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient adjustment rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientAdjustmentRollupWorkflow(ILenientAdjustmentRollupStore Store, LenientAdjustmentRollupValidator Validator, IClock Clock, List<string> Journal, LenientAdjustmentRollupEngine Engine)
{
    public static LenientAdjustmentRollupWorkflow NewLenientAdjustmentRollupWorkflow(ILenientAdjustmentRollupStore store, IClock clock, Registry flags)
    {
        return new LenientAdjustmentRollupWorkflow(
            store,
            LenientAdjustmentRollupRules.DefaultLenientAdjustmentRollupValidator(),
            clock,
            new List<string>(),
            LenientAdjustmentRollupEngine.DefaultLenientAdjustmentRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient adjustment rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientAdjustmentRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient adjustment rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientAdjustmentRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient adjustment rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
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
