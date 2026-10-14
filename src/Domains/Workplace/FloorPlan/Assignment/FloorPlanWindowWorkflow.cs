namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanWindowDesk(IFloorPlanWindowStore Store, FloorPlanWindowValidator Validator, IClock Clock, List<string> Journal, FloorPlanWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanWindowStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanWindowStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan window {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of floor plan window workflow.</summary>
public static class FloorPlanWindowWorkflow
{
    public static FloorPlanWindowDesk NewFloorPlanWindowDesk(IFloorPlanWindowStore store, IClock clock, Registry flags)
    {
        return new FloorPlanWindowDesk(
            store,
            FloorPlanWindowRules.DefaultFloorPlanWindowValidator(),
            clock,
            new List<string>(),
            FloorPlanWindowEngine.DefaultFloorPlanWindowEngine(flags));
    }
}
