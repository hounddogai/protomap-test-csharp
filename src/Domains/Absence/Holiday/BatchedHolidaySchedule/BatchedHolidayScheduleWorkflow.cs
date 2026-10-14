namespace Acme.Workforce.Absence.Holiday.BatchedHolidaySchedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves batched holiday schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedHolidayScheduleDesk(IBatchedHolidayScheduleStore Store, BatchedHolidayScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched holiday schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedHolidayScheduleStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched holiday schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedHolidayScheduleStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of batched holiday schedule workflow.</summary>
public static class BatchedHolidayScheduleWorkflow
{
    public static BatchedHolidayScheduleDesk NewBatchedHolidayScheduleDesk(IBatchedHolidayScheduleStore store, IClock clock)
    {
        return new BatchedHolidayScheduleDesk(
            store,
            BatchedHolidayScheduleRules.DefaultBatchedHolidayScheduleValidator(),
            clock,
            new List<string>());
    }
}
