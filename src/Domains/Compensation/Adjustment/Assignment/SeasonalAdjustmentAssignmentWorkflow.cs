namespace Acme.Workforce.Compensation.Adjustment.Assignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves seasonal adjustment assignments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record SeasonalAdjustmentAssignmentManager(ISeasonalAdjustmentAssignmentStore Store, SeasonalAdjustmentAssignmentValidator Validator, IClock Clock, List<string> Journal, SeasonalAdjustmentAssignmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalAdjustmentAssignmentStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalAdjustmentAssignmentStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal adjustment assignment {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of seasonal adjustment assignment workflow.</summary>
public static class SeasonalAdjustmentAssignmentWorkflow
{
    public static SeasonalAdjustmentAssignmentManager NewSeasonalAdjustmentAssignmentManager(ISeasonalAdjustmentAssignmentStore store, IClock clock, Registry flags)
    {
        return new SeasonalAdjustmentAssignmentManager(
            store,
            SeasonalAdjustmentAssignmentRules.DefaultSeasonalAdjustmentAssignmentValidator(),
            clock,
            new List<string>(),
            SeasonalAdjustmentAssignmentEngine.DefaultSeasonalAdjustmentAssignmentEngine(flags));
    }
}
