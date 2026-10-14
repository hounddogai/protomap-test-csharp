namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted booking entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record WeightedBookingEntryManager(IWeightedBookingEntryStore Store, WeightedBookingEntryValidator Validator, IClock Clock, List<string> Journal, WeightedBookingEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted booking entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedBookingEntryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted booking entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedBookingEntryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted booking entry {code} not found");
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

/// <summary>Functions of weighted booking entry workflow.</summary>
public static class WeightedBookingEntryWorkflow
{
    public static WeightedBookingEntryManager NewWeightedBookingEntryManager(IWeightedBookingEntryStore store, IClock clock)
    {
        return new WeightedBookingEntryManager(
            store,
            WeightedBookingEntryRules.DefaultWeightedBookingEntryValidator(),
            clock,
            new List<string>(),
            WeightedBookingEntryEngine.DefaultWeightedBookingEntryEngine());
    }
}
