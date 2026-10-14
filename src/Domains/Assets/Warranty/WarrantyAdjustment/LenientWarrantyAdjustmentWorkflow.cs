namespace Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient warranty adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientWarrantyAdjustmentCoordinator(ILenientWarrantyAdjustmentStore Store, LenientWarrantyAdjustmentValidator Validator, IClock Clock, List<string> Journal, LenientWarrantyAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient warranty adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientWarrantyAdjustmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient warranty adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientWarrantyAdjustmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient warranty adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of lenient warranty adjustment workflow.</summary>
public static class LenientWarrantyAdjustmentWorkflow
{
    public static LenientWarrantyAdjustmentCoordinator NewLenientWarrantyAdjustmentCoordinator(ILenientWarrantyAdjustmentStore store, IClock clock, Registry flags)
    {
        return new LenientWarrantyAdjustmentCoordinator(
            store,
            LenientWarrantyAdjustmentRules.DefaultLenientWarrantyAdjustmentValidator(),
            clock,
            new List<string>(),
            LenientWarrantyAdjustmentEngine.DefaultLenientWarrantyAdjustmentEngine(flags));
    }
}
