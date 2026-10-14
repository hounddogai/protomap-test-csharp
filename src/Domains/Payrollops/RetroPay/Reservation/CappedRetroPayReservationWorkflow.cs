namespace Acme.Workforce.Payrollops.RetroPay.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped retro pay reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedRetroPayReservationManager(ICappedRetroPayReservationStore Store, CappedRetroPayReservationValidator Validator, IClock Clock, List<string> Journal, CappedRetroPayReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped retro pay reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedRetroPayReservationStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped retro pay reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedRetroPayReservationStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped retro pay reservation {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of capped retro pay reservation workflow.</summary>
public static class CappedRetroPayReservationWorkflow
{
    public static CappedRetroPayReservationManager NewCappedRetroPayReservationManager(ICappedRetroPayReservationStore store, IClock clock, Registry flags)
    {
        return new CappedRetroPayReservationManager(
            store,
            CappedRetroPayReservationRules.DefaultCappedRetroPayReservationValidator(),
            clock,
            new List<string>(),
            CappedRetroPayReservationEngine.DefaultCappedRetroPayReservationEngine(flags));
    }
}
