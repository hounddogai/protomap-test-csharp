namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves return plan checkpoints through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReturnPlanCheckpointManager(IReturnPlanCheckpointStore Store, ReturnPlanCheckpointValidator Validator, IClock Clock, List<string> Journal, ReturnPlanCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReturnPlanCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReturnPlanCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"return plan checkpoint {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of return plan checkpoint workflow.</summary>
public static class ReturnPlanCheckpointWorkflow
{
    public static ReturnPlanCheckpointManager NewReturnPlanCheckpointManager(IReturnPlanCheckpointStore store, IClock clock, Registry flags)
    {
        return new ReturnPlanCheckpointManager(
            store,
            ReturnPlanCheckpointRules.DefaultReturnPlanCheckpointValidator(),
            clock,
            new List<string>(),
            ReturnPlanCheckpointEngine.DefaultReturnPlanCheckpointEngine(flags));
    }
}
