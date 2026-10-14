namespace Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves capped pay run reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedPayRunReservationCoordinator(ICappedPayRunReservationStore Store, CappedPayRunReservationValidator Validator, IClock Clock, List<string> Journal, CappedPayRunReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay run reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedPayRunReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay run reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedPayRunReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped pay run reservation {code} not found");
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

/// <summary>Functions of capped pay run reservation workflow.</summary>
public static class CappedPayRunReservationWorkflow
{
    public static CappedPayRunReservationCoordinator NewCappedPayRunReservationCoordinator(ICappedPayRunReservationStore store, IClock clock)
    {
        return new CappedPayRunReservationCoordinator(
            store,
            CappedPayRunReservationRules.DefaultCappedPayRunReservationValidator(),
            clock,
            new List<string>(),
            CappedPayRunReservationEngine.DefaultCappedPayRunReservationEngine());
    }
}
