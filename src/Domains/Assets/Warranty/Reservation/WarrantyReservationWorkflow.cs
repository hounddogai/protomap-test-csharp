namespace Acme.Workforce.Assets.Warranty.Reservation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves warranty reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record WarrantyReservationService(IWarrantyReservationStore Store, WarrantyReservationValidator Validator, IClock Clock, List<string> Journal, WarrantyReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WarrantyReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WarrantyReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"warranty reservation {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of warranty reservation workflow.</summary>
public static class WarrantyReservationWorkflow
{
    public static WarrantyReservationService NewWarrantyReservationService(IWarrantyReservationStore store, IClock clock)
    {
        return new WarrantyReservationService(
            store,
            WarrantyReservationRules.DefaultWarrantyReservationValidator(),
            clock,
            new List<string>(),
            WarrantyReservationEngine.DefaultWarrantyReservationEngine());
    }
}
