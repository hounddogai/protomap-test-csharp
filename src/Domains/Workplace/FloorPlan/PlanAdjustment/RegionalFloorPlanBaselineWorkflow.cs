namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional floor plan baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record RegionalFloorPlanBaselineManager(IRegionalFloorPlanBaselineStore Store, RegionalFloorPlanBaselineValidator Validator, IClock Clock, List<string> Journal, RegionalFloorPlanBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional floor plan baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalFloorPlanBaselineStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional floor plan baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalFloorPlanBaselineStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional floor plan baseline {code} not found");
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

/// <summary>Functions of regional floor plan baseline workflow.</summary>
public static class RegionalFloorPlanBaselineWorkflow
{
    public static RegionalFloorPlanBaselineManager NewRegionalFloorPlanBaselineManager(IRegionalFloorPlanBaselineStore store, IClock clock, Registry flags)
    {
        return new RegionalFloorPlanBaselineManager(
            store,
            RegionalFloorPlanBaselineRules.DefaultRegionalFloorPlanBaselineValidator(),
            clock,
            new List<string>(),
            RegionalFloorPlanBaselineEngine.DefaultRegionalFloorPlanBaselineEngine(flags));
    }
}
