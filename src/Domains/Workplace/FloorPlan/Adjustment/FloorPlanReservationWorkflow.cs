namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan reservations through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanReservationWorkflow(IFloorPlanReservationStore Store, FloorPlanReservationValidator Validator, IClock Clock, List<string> Journal, FloorPlanReservationEngine Engine)
{
    public static FloorPlanReservationWorkflow NewFloorPlanReservationWorkflow(IFloorPlanReservationStore store, IClock clock, Registry flags)
    {
        return new FloorPlanReservationWorkflow(
            store,
            FloorPlanReservationRules.DefaultFloorPlanReservationValidator(),
            clock,
            new List<string>(),
            FloorPlanReservationEngine.DefaultFloorPlanReservationEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanReservationStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanReservationStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan reservation {code} not found");
        }
        var result = Engine.Run(item);
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
