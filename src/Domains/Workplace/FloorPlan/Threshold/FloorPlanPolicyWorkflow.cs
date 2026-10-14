namespace Acme.Workforce.Workplace.FloorPlan.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves floor plan policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record FloorPlanPolicyManager(IFloorPlanPolicyStore Store, FloorPlanPolicyValidator Validator, IClock Clock, List<string> Journal, FloorPlanPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FloorPlanPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FloorPlanPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"floor plan policy {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of floor plan policy workflow.</summary>
public static class FloorPlanPolicyWorkflow
{
    public static FloorPlanPolicyManager NewFloorPlanPolicyManager(IFloorPlanPolicyStore store, IClock clock, Registry flags)
    {
        return new FloorPlanPolicyManager(
            store,
            FloorPlanPolicyRules.DefaultFloorPlanPolicyValidator(),
            clock,
            new List<string>(),
            FloorPlanPolicyEngine.DefaultFloorPlanPolicyEngine(flags));
    }
}
