namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback merit cycle checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackMeritCycleCheckpointManager(IFallbackMeritCycleCheckpointStore Store, FallbackMeritCycleCheckpointValidator Validator, IClock Clock, List<string> Journal, FallbackMeritCycleCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback merit cycle checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackMeritCycleCheckpointStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback merit cycle checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackMeritCycleCheckpointStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback merit cycle checkpoint {code} not found");
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

/// <summary>Functions of fallback merit cycle checkpoint workflow.</summary>
public static class FallbackMeritCycleCheckpointWorkflow
{
    public static FallbackMeritCycleCheckpointManager NewFallbackMeritCycleCheckpointManager(IFallbackMeritCycleCheckpointStore store, IClock clock, Registry flags)
    {
        return new FallbackMeritCycleCheckpointManager(
            store,
            FallbackMeritCycleCheckpointRules.DefaultFallbackMeritCycleCheckpointValidator(),
            clock,
            new List<string>(),
            FallbackMeritCycleCheckpointEngine.DefaultFallbackMeritCycleCheckpointEngine(flags));
    }
}
