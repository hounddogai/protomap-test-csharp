namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered booking records through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredBookingRecordCoordinator(ITieredBookingRecordStore Store, TieredBookingRecordValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered booking record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredBookingRecordStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered booking record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredBookingRecordStatus.Retired);
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

/// <summary>Functions of tiered booking record workflow.</summary>
public static class TieredBookingRecordWorkflow
{
    public static TieredBookingRecordCoordinator NewTieredBookingRecordCoordinator(ITieredBookingRecordStore store, IClock clock)
    {
        return new TieredBookingRecordCoordinator(
            store,
            TieredBookingRecordRules.DefaultTieredBookingRecordValidator(),
            clock,
            new List<string>());
    }
}
