namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict floor plan bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictFloorPlanBundleService(IStrictFloorPlanBundleStore Store, StrictFloorPlanBundleValidator Validator, IClock Clock, List<string> Journal, StrictFloorPlanBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict floor plan bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictFloorPlanBundleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict floor plan bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictFloorPlanBundleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict floor plan bundle {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of strict floor plan bundle workflow.</summary>
public static class StrictFloorPlanBundleWorkflow
{
    public static StrictFloorPlanBundleService NewStrictFloorPlanBundleService(IStrictFloorPlanBundleStore store, IClock clock, Registry flags)
    {
        return new StrictFloorPlanBundleService(
            store,
            StrictFloorPlanBundleRules.DefaultStrictFloorPlanBundleValidator(),
            clock,
            new List<string>(),
            StrictFloorPlanBundleEngine.DefaultStrictFloorPlanBundleEngine(flags));
    }
}
