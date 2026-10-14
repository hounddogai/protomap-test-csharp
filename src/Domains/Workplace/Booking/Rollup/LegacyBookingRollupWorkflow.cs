namespace Acme.Workforce.Workplace.Booking.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy booking rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyBookingRollupWorkflow(ILegacyBookingRollupStore Store, LegacyBookingRollupValidator Validator, IClock Clock, List<string> Journal, LegacyBookingRollupEngine Engine)
{
    public static LegacyBookingRollupWorkflow NewLegacyBookingRollupWorkflow(ILegacyBookingRollupStore store, IClock clock, Registry flags)
    {
        return new LegacyBookingRollupWorkflow(
            store,
            LegacyBookingRollupRules.DefaultLegacyBookingRollupValidator(),
            clock,
            new List<string>(),
            LegacyBookingRollupEngine.DefaultLegacyBookingRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy booking rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyBookingRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy booking rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyBookingRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy booking rollup {code} not found");
        }
        var result = Engine.Price(item);
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
