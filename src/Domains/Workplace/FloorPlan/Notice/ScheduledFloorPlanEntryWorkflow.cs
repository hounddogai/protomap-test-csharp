namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves scheduled floor plan entries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ScheduledFloorPlanEntryWorkflow(IScheduledFloorPlanEntryStore Store, ScheduledFloorPlanEntryValidator Validator, IClock Clock, List<string> Journal, ScheduledFloorPlanEntryEngine Engine)
{
    public static ScheduledFloorPlanEntryWorkflow NewScheduledFloorPlanEntryWorkflow(IScheduledFloorPlanEntryStore store, IClock clock, Registry flags)
    {
        return new ScheduledFloorPlanEntryWorkflow(
            store,
            ScheduledFloorPlanEntryRules.DefaultScheduledFloorPlanEntryValidator(),
            clock,
            new List<string>(),
            ScheduledFloorPlanEntryEngine.DefaultScheduledFloorPlanEntryEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled floor plan entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledFloorPlanEntryStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled floor plan entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledFloorPlanEntryStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled floor plan entry {code} not found");
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
