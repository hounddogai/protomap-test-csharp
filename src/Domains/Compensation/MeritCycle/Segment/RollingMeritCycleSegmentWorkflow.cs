namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling merit cycle segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RollingMeritCycleSegmentManager(IRollingMeritCycleSegmentStore Store, RollingMeritCycleSegmentValidator Validator, IClock Clock, List<string> Journal, RollingMeritCycleSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling merit cycle segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingMeritCycleSegmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling merit cycle segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingMeritCycleSegmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling merit cycle segment {code} not found");
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

/// <summary>Functions of rolling merit cycle segment workflow.</summary>
public static class RollingMeritCycleSegmentWorkflow
{
    public static RollingMeritCycleSegmentManager NewRollingMeritCycleSegmentManager(IRollingMeritCycleSegmentStore store, IClock clock, Registry flags)
    {
        return new RollingMeritCycleSegmentManager(
            store,
            RollingMeritCycleSegmentRules.DefaultRollingMeritCycleSegmentValidator(),
            clock,
            new List<string>(),
            RollingMeritCycleSegmentEngine.DefaultRollingMeritCycleSegmentEngine(flags));
    }
}
