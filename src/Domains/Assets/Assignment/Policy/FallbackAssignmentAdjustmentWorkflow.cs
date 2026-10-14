namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback assignment adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackAssignmentAdjustmentManager(IFallbackAssignmentAdjustmentStore Store, FallbackAssignmentAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback assignment adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAssignmentAdjustmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback assignment adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAssignmentAdjustmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of fallback assignment adjustment workflow.</summary>
public static class FallbackAssignmentAdjustmentWorkflow
{
    public static FallbackAssignmentAdjustmentManager NewFallbackAssignmentAdjustmentManager(IFallbackAssignmentAdjustmentStore store, IClock clock)
    {
        return new FallbackAssignmentAdjustmentManager(
            store,
            FallbackAssignmentAdjustmentRules.DefaultFallbackAssignmentAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
