namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict adjustment baselines through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record StrictAdjustmentBaselineWorkflow(IStrictAdjustmentBaselineStore Store, StrictAdjustmentBaselineValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictAdjustmentBaselineWorkflow NewStrictAdjustmentBaselineWorkflow(IStrictAdjustmentBaselineStore store, IClock clock)
    {
        return new StrictAdjustmentBaselineWorkflow(
            store,
            StrictAdjustmentBaselineRules.DefaultStrictAdjustmentBaselineValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict adjustment baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictAdjustmentBaselineStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict adjustment baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictAdjustmentBaselineStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
