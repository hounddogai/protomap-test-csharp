namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional holiday adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalHolidayAdjustmentManager(IRegionalHolidayAdjustmentStore Store, RegionalHolidayAdjustmentValidator Validator, IClock Clock, List<string> Journal, RegionalHolidayAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalHolidayAdjustmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalHolidayAdjustmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of regional holiday adjustment workflow.</summary>
public static class RegionalHolidayAdjustmentWorkflow
{
    public static RegionalHolidayAdjustmentManager NewRegionalHolidayAdjustmentManager(IRegionalHolidayAdjustmentStore store, IClock clock, Registry flags)
    {
        return new RegionalHolidayAdjustmentManager(
            store,
            RegionalHolidayAdjustmentRules.DefaultRegionalHolidayAdjustmentValidator(),
            clock,
            new List<string>(),
            RegionalHolidayAdjustmentEngine.DefaultRegionalHolidayAdjustmentEngine(flags));
    }
}
