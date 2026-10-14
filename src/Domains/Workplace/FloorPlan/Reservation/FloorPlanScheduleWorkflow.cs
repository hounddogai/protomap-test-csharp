namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanScheduleDesk(IFloorPlanScheduleStore Store, FloorPlanScheduleValidator Validator, IClock Clock, List<string> Journal, FloorPlanScheduleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan schedule {code} not found");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of floor plan schedule workflow.</summary>
public static class FloorPlanScheduleWorkflow
{
    public static FloorPlanScheduleDesk NewFloorPlanScheduleDesk(IFloorPlanScheduleStore store, IClock clock, Registry flags)
    {
        return new FloorPlanScheduleDesk(
            store,
            FloorPlanScheduleRules.DefaultFloorPlanScheduleValidator(),
            clock,
            new List<string>(),
            FloorPlanScheduleEngine.DefaultFloorPlanScheduleEngine(flags));
    }
}
