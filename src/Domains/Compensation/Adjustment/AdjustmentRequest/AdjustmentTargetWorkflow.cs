namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentTargetService(IAdjustmentTargetStore Store, AdjustmentTargetValidator Validator, IClock Clock, List<string> Journal, AdjustmentTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentTargetStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentTargetStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment target {code} not found");
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

/// <summary>Functions of adjustment target workflow.</summary>
public static class AdjustmentTargetWorkflow
{
    public static AdjustmentTargetService NewAdjustmentTargetService(IAdjustmentTargetStore store, IClock clock)
    {
        return new AdjustmentTargetService(
            store,
            AdjustmentTargetRules.DefaultAdjustmentTargetValidator(),
            clock,
            new List<string>(),
            AdjustmentTargetEngine.DefaultAdjustmentTargetEngine());
    }
}
