namespace Acme.Workforce.Orgdesign.CostModel.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves cost model reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record CostModelReservationWorkflow(ICostModelReservationStore Store, CostModelReservationValidator Validator, IClock Clock, List<string> Journal, CostModelReservationEngine Engine)
{
    public static CostModelReservationWorkflow NewCostModelReservationWorkflow(ICostModelReservationStore store, IClock clock, Registry flags)
    {
        return new CostModelReservationWorkflow(
            store,
            CostModelReservationRules.DefaultCostModelReservationValidator(),
            clock,
            new List<string>(),
            CostModelReservationEngine.DefaultCostModelReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CostModelReservationStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CostModelReservationStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"cost model reservation {code} not found");
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
