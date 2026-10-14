namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopReservationService(ILaptopReservationStore Store, LaptopReservationValidator Validator, IClock Clock, List<string> Journal, LaptopReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopReservationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopReservationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of laptop reservation workflow.</summary>
public static class LaptopReservationWorkflow
{
    public static LaptopReservationService NewLaptopReservationService(ILaptopReservationStore store, IClock clock)
    {
        return new LaptopReservationService(
            store,
            LaptopReservationRules.DefaultLaptopReservationValidator(),
            clock,
            new List<string>(),
            LaptopReservationEngine.DefaultLaptopReservationEngine());
    }
}
