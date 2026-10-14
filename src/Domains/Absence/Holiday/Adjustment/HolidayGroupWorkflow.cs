namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayGroupManager(IHolidayGroupStore Store, HolidayGroupValidator Validator, IClock Clock, List<string> Journal, HolidayGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayGroupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayGroupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday group {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of holiday group workflow.</summary>
public static class HolidayGroupWorkflow
{
    public static HolidayGroupManager NewHolidayGroupManager(IHolidayGroupStore store, IClock clock)
    {
        return new HolidayGroupManager(
            store,
            HolidayGroupRules.DefaultHolidayGroupValidator(),
            clock,
            new List<string>(),
            HolidayGroupEngine.DefaultHolidayGroupEngine());
    }
}
