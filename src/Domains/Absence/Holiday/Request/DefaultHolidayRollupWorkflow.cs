namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default holiday rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultHolidayRollupService(IDefaultHolidayRollupStore Store, DefaultHolidayRollupValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultHolidayRollupStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default holiday rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultHolidayRollupStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of default holiday rollup workflow.</summary>
public static class DefaultHolidayRollupWorkflow
{
    public static DefaultHolidayRollupService NewDefaultHolidayRollupService(IDefaultHolidayRollupStore store, IClock clock)
    {
        return new DefaultHolidayRollupService(
            store,
            DefaultHolidayRollupRules.DefaultDefaultHolidayRollupValidator(),
            clock,
            new List<string>());
    }
}
