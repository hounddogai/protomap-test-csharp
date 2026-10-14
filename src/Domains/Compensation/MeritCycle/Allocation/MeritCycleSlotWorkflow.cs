namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleSlotCoordinator(IMeritCycleSlotStore Store, MeritCycleSlotValidator Validator, IClock Clock, List<string> Journal, MeritCycleSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleSlotStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleSlotStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle slot {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of merit cycle slot workflow.</summary>
public static class MeritCycleSlotWorkflow
{
    public static MeritCycleSlotCoordinator NewMeritCycleSlotCoordinator(IMeritCycleSlotStore store, IClock clock, Registry flags)
    {
        return new MeritCycleSlotCoordinator(
            store,
            MeritCycleSlotRules.DefaultMeritCycleSlotValidator(),
            clock,
            new List<string>(),
            MeritCycleSlotEngine.DefaultMeritCycleSlotEngine(flags));
    }
}
