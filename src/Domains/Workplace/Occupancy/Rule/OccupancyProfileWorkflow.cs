namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves occupancy profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record OccupancyProfileWorkflow(IOccupancyProfileStore Store, OccupancyProfileValidator Validator, IClock Clock, List<string> Journal, OccupancyProfileEngine Engine)
{
    public static OccupancyProfileWorkflow NewOccupancyProfileWorkflow(IOccupancyProfileStore store, IClock clock, Registry flags)
    {
        return new OccupancyProfileWorkflow(
            store,
            OccupancyProfileRules.DefaultOccupancyProfileValidator(),
            clock,
            new List<string>(),
            OccupancyProfileEngine.DefaultOccupancyProfileEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(OccupancyProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(OccupancyProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"occupancy profile {code} not found");
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
