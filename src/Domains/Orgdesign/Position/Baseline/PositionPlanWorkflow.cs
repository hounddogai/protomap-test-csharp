namespace Acme.Workforce.Orgdesign.Position.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionPlanService(IPositionPlanStore Store, PositionPlanValidator Validator, IClock Clock, List<string> Journal, PositionPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionPlanStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionPlanStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position plan {code} not found");
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

/// <summary>Functions of position plan workflow.</summary>
public static class PositionPlanWorkflow
{
    public static PositionPlanService NewPositionPlanService(IPositionPlanStore store, IClock clock, Registry flags)
    {
        return new PositionPlanService(
            store,
            PositionPlanRules.DefaultPositionPlanValidator(),
            clock,
            new List<string>(),
            PositionPlanEngine.DefaultPositionPlanEngine(flags));
    }
}
