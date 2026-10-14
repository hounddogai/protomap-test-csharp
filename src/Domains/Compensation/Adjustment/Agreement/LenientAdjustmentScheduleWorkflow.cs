namespace Acme.Workforce.Compensation.Adjustment.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient adjustment schedules through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientAdjustmentScheduleCoordinator(ILenientAdjustmentScheduleStore Store, LenientAdjustmentScheduleValidator Validator, IClock Clock, List<string> Journal, LenientAdjustmentScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient adjustment schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientAdjustmentScheduleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient adjustment schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientAdjustmentScheduleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient adjustment schedule {code} not found");
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

/// <summary>Functions of lenient adjustment schedule workflow.</summary>
public static class LenientAdjustmentScheduleWorkflow
{
    public static LenientAdjustmentScheduleCoordinator NewLenientAdjustmentScheduleCoordinator(ILenientAdjustmentScheduleStore store, IClock clock, Registry flags)
    {
        return new LenientAdjustmentScheduleCoordinator(
            store,
            LenientAdjustmentScheduleRules.DefaultLenientAdjustmentScheduleValidator(),
            clock,
            new List<string>(),
            LenientAdjustmentScheduleEngine.DefaultLenientAdjustmentScheduleEngine(flags));
    }
}
