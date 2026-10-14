namespace Acme.Workforce.Assets.LicenseSeat.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves license seat adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record LicenseSeatAdjustmentService(ILicenseSeatAdjustmentStore Store, LicenseSeatAdjustmentValidator Validator, IClock Clock, List<string> Journal, LicenseSeatAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LicenseSeatAdjustmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LicenseSeatAdjustmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"license seat adjustment {code} not found");
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

/// <summary>Functions of license seat adjustment workflow.</summary>
public static class LicenseSeatAdjustmentWorkflow
{
    public static LicenseSeatAdjustmentService NewLicenseSeatAdjustmentService(ILicenseSeatAdjustmentStore store, IClock clock, Registry flags)
    {
        return new LicenseSeatAdjustmentService(
            store,
            LicenseSeatAdjustmentRules.DefaultLicenseSeatAdjustmentValidator(),
            clock,
            new List<string>(),
            LicenseSeatAdjustmentEngine.DefaultLicenseSeatAdjustmentEngine(flags));
    }
}
