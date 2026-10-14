namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual floor plan reservations through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualFloorPlanReservationDesk(IManualFloorPlanReservationStore Store, ManualFloorPlanReservationValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual floor plan reservation {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualFloorPlanReservationStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual floor plan reservation {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualFloorPlanReservationStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of manual floor plan reservation workflow.</summary>
public static class ManualFloorPlanReservationWorkflow
{
    public static ManualFloorPlanReservationDesk NewManualFloorPlanReservationDesk(IManualFloorPlanReservationStore store, IClock clock)
    {
        return new ManualFloorPlanReservationDesk(
            store,
            ManualFloorPlanReservationRules.DefaultManualFloorPlanReservationValidator(),
            clock,
            new List<string>());
    }
}
