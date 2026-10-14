namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves floor plan drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanDraftDesk(IFloorPlanDraftStore Store, FloorPlanDraftValidator Validator, IClock Clock, List<string> Journal, FloorPlanDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanDraftStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanDraftStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan draft {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of floor plan draft workflow.</summary>
public static class FloorPlanDraftWorkflow
{
    public static FloorPlanDraftDesk NewFloorPlanDraftDesk(IFloorPlanDraftStore store, IClock clock)
    {
        return new FloorPlanDraftDesk(
            store,
            FloorPlanDraftRules.DefaultFloorPlanDraftValidator(),
            clock,
            new List<string>(),
            FloorPlanDraftEngine.DefaultFloorPlanDraftEngine());
    }
}
