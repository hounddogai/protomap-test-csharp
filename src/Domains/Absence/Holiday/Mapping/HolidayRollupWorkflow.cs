namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves holiday rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayRollupManager(IHolidayRollupStore Store, HolidayRollupValidator Validator, IClock Clock, List<string> Journal, HolidayRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday rollup {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of holiday rollup workflow.</summary>
public static class HolidayRollupWorkflow
{
    public static HolidayRollupManager NewHolidayRollupManager(IHolidayRollupStore store, IClock clock, Registry flags)
    {
        return new HolidayRollupManager(
            store,
            HolidayRollupRules.DefaultHolidayRollupValidator(),
            clock,
            new List<string>(),
            HolidayRollupEngine.DefaultHolidayRollupEngine(flags));
    }
}
