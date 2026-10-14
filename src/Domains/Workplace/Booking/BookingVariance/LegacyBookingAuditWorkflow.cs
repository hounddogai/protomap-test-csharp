namespace Acme.Workforce.Workplace.Booking.BookingVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy booking audits through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyBookingAuditCoordinator(ILegacyBookingAuditStore Store, LegacyBookingAuditValidator Validator, IClock Clock, List<string> Journal, LegacyBookingAuditEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy booking audit {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyBookingAuditStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy booking audit {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyBookingAuditStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy booking audit {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of legacy booking audit workflow.</summary>
public static class LegacyBookingAuditWorkflow
{
    public static LegacyBookingAuditCoordinator NewLegacyBookingAuditCoordinator(ILegacyBookingAuditStore store, IClock clock, Registry flags)
    {
        return new LegacyBookingAuditCoordinator(
            store,
            LegacyBookingAuditRules.DefaultLegacyBookingAuditValidator(),
            clock,
            new List<string>(),
            LegacyBookingAuditEngine.DefaultLegacyBookingAuditEngine(flags));
    }
}
