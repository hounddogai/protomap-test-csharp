namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanReviewManager(IFloorPlanReviewStore Store, FloorPlanReviewValidator Validator, IClock Clock, List<string> Journal, FloorPlanReviewEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanReviewStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanReviewStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan review {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of floor plan review workflow.</summary>
public static class FloorPlanReviewWorkflow
{
    public static FloorPlanReviewManager NewFloorPlanReviewManager(IFloorPlanReviewStore store, IClock clock)
    {
        return new FloorPlanReviewManager(
            store,
            FloorPlanReviewRules.DefaultFloorPlanReviewValidator(),
            clock,
            new List<string>(),
            FloorPlanReviewEngine.DefaultFloorPlanReviewEngine());
    }
}
