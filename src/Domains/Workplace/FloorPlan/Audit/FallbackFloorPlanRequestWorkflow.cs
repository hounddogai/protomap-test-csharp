namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves fallback floor plan requests through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackFloorPlanRequestService(IFallbackFloorPlanRequestStore Store, FallbackFloorPlanRequestValidator Validator, IClock Clock, List<string> Journal, FallbackFloorPlanRequestEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback floor plan request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackFloorPlanRequestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback floor plan request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackFloorPlanRequestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback floor plan request {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of fallback floor plan request workflow.</summary>
public static class FallbackFloorPlanRequestWorkflow
{
    public static FallbackFloorPlanRequestService NewFallbackFloorPlanRequestService(IFallbackFloorPlanRequestStore store, IClock clock, Registry flags)
    {
        return new FallbackFloorPlanRequestService(
            store,
            FallbackFloorPlanRequestRules.DefaultFallbackFloorPlanRequestValidator(),
            clock,
            new List<string>(),
            FallbackFloorPlanRequestEngine.DefaultFallbackFloorPlanRequestEngine(flags));
    }
}
