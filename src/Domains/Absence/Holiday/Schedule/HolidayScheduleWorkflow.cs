namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves holiday schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record HolidayScheduleCoordinator(IHolidayScheduleStore Store, HolidayScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HolidayScheduleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"holiday schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HolidayScheduleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of holiday schedule workflow.</summary>
public static class HolidayScheduleWorkflow
{
    public static HolidayScheduleCoordinator NewHolidayScheduleCoordinator(IHolidayScheduleStore store, IClock clock)
    {
        return new HolidayScheduleCoordinator(
            store,
            HolidayScheduleRules.DefaultHolidayScheduleValidator(),
            clock,
            new List<string>());
    }
}
