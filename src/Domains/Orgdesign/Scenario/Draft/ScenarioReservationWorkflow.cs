namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scenario reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScenarioReservationManager(IScenarioReservationStore Store, ScenarioReservationValidator Validator, IClock Clock, List<string> Journal, ScenarioReservationEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScenarioReservationStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScenarioReservationStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scenario reservation {code} not found");
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

/// <summary>Functions of scenario reservation workflow.</summary>
public static class ScenarioReservationWorkflow
{
    public static ScenarioReservationManager NewScenarioReservationManager(IScenarioReservationStore store, IClock clock, Registry flags)
    {
        return new ScenarioReservationManager(
            store,
            ScenarioReservationRules.DefaultScenarioReservationValidator(),
            clock,
            new List<string>(),
            ScenarioReservationEngine.DefaultScenarioReservationEngine(flags));
    }
}
