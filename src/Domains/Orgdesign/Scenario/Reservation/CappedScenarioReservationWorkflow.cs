namespace Acme.Workforce.Orgdesign.Scenario.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped scenario reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record CappedScenarioReservationCoordinator(ICappedScenarioReservationStore Store, CappedScenarioReservationValidator Validator, IClock Clock, List<string> Journal, CappedScenarioReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped scenario reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedScenarioReservationStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped scenario reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedScenarioReservationStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped scenario reservation {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of capped scenario reservation workflow.</summary>
public static class CappedScenarioReservationWorkflow
{
    public static CappedScenarioReservationCoordinator NewCappedScenarioReservationCoordinator(ICappedScenarioReservationStore store, IClock clock, Registry flags)
    {
        return new CappedScenarioReservationCoordinator(
            store,
            CappedScenarioReservationRules.DefaultCappedScenarioReservationValidator(),
            clock,
            new List<string>(),
            CappedScenarioReservationEngine.DefaultCappedScenarioReservationEngine(flags));
    }
}
