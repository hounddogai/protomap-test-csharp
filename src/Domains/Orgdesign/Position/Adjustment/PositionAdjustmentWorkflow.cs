namespace Acme.Workforce.Orgdesign.Position.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves position adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record PositionAdjustmentWorkflow(IPositionAdjustmentStore Store, PositionAdjustmentValidator Validator, IClock Clock, List<string> Journal, PositionAdjustmentEngine Engine)
{
    public static PositionAdjustmentWorkflow NewPositionAdjustmentWorkflow(IPositionAdjustmentStore store, IClock clock, Registry flags)
    {
        return new PositionAdjustmentWorkflow(
            store,
            PositionAdjustmentRules.DefaultPositionAdjustmentValidator(),
            clock,
            new List<string>(),
            PositionAdjustmentEngine.DefaultPositionAdjustmentEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PositionAdjustmentStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PositionAdjustmentStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"position adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
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
