namespace Acme.Workforce.Contractors.Vendor.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled vendor schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledVendorScheduleDesk(IScheduledVendorScheduleStore Store, ScheduledVendorScheduleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled vendor schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledVendorScheduleStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled vendor schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledVendorScheduleStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of scheduled vendor schedule workflow.</summary>
public static class ScheduledVendorScheduleWorkflow
{
    public static ScheduledVendorScheduleDesk NewScheduledVendorScheduleDesk(IScheduledVendorScheduleStore store, IClock clock)
    {
        return new ScheduledVendorScheduleDesk(
            store,
            ScheduledVendorScheduleRules.DefaultScheduledVendorScheduleValidator(),
            clock,
            new List<string>());
    }
}
