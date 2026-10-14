namespace Acme.Workforce.Absence.LeaveRequest.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted leave request adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedLeaveRequestAdjustmentCoordinator(IWeightedLeaveRequestAdjustmentStore Store, WeightedLeaveRequestAdjustmentValidator Validator, IClock Clock, List<string> Journal, WeightedLeaveRequestAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted leave request adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedLeaveRequestAdjustmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted leave request adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedLeaveRequestAdjustmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted leave request adjustment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of weighted leave request adjustment workflow.</summary>
public static class WeightedLeaveRequestAdjustmentWorkflow
{
    public static WeightedLeaveRequestAdjustmentCoordinator NewWeightedLeaveRequestAdjustmentCoordinator(IWeightedLeaveRequestAdjustmentStore store, IClock clock)
    {
        return new WeightedLeaveRequestAdjustmentCoordinator(
            store,
            WeightedLeaveRequestAdjustmentRules.DefaultWeightedLeaveRequestAdjustmentValidator(),
            clock,
            new List<string>(),
            WeightedLeaveRequestAdjustmentEngine.DefaultWeightedLeaveRequestAdjustmentEngine());
    }
}
