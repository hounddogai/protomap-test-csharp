namespace Acme.Workforce.Payrollops.TaxForm.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tax form reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record TaxFormReservationCoordinator(ITaxFormReservationStore Store, TaxFormReservationValidator Validator, IClock Clock, List<string> Journal, TaxFormReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TaxFormReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TaxFormReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tax form reservation {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of tax form reservation workflow.</summary>
public static class TaxFormReservationWorkflow
{
    public static TaxFormReservationCoordinator NewTaxFormReservationCoordinator(ITaxFormReservationStore store, IClock clock, Registry flags)
    {
        return new TaxFormReservationCoordinator(
            store,
            TaxFormReservationRules.DefaultTaxFormReservationValidator(),
            clock,
            new List<string>(),
            TaxFormReservationEngine.DefaultTaxFormReservationEngine(flags));
    }
}
