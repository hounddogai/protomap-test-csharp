namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual booking entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualBookingEntryService(IManualBookingEntryStore Store, ManualBookingEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual booking entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualBookingEntryStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual booking entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualBookingEntryStatus.Retired);
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

/// <summary>Functions of manual booking entry workflow.</summary>
public static class ManualBookingEntryWorkflow
{
    public static ManualBookingEntryService NewManualBookingEntryService(IManualBookingEntryStore store, IClock clock)
    {
        return new ManualBookingEntryService(
            store,
            ManualBookingEntryRules.DefaultManualBookingEntryValidator(),
            clock,
            new List<string>());
    }
}
