namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy merit cycle checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyMeritCycleCheckpointService(ILegacyMeritCycleCheckpointStore Store, LegacyMeritCycleCheckpointValidator Validator, IClock Clock, List<string> Journal, LegacyMeritCycleCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy merit cycle checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyMeritCycleCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy merit cycle checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyMeritCycleCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy merit cycle checkpoint {code} not found");
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

/// <summary>Functions of legacy merit cycle checkpoint workflow.</summary>
public static class LegacyMeritCycleCheckpointWorkflow
{
    public static LegacyMeritCycleCheckpointService NewLegacyMeritCycleCheckpointService(ILegacyMeritCycleCheckpointStore store, IClock clock, Registry flags)
    {
        return new LegacyMeritCycleCheckpointService(
            store,
            LegacyMeritCycleCheckpointRules.DefaultLegacyMeritCycleCheckpointValidator(),
            clock,
            new List<string>(),
            LegacyMeritCycleCheckpointEngine.DefaultLegacyMeritCycleCheckpointEngine(flags));
    }
}
