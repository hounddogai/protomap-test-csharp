namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending visitor adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingVisitorAdjustmentWorkflow(IPendingVisitorAdjustmentStore Store, PendingVisitorAdjustmentValidator Validator, IClock Clock, List<string> Journal, PendingVisitorAdjustmentEngine Engine)
{
    public static PendingVisitorAdjustmentWorkflow NewPendingVisitorAdjustmentWorkflow(IPendingVisitorAdjustmentStore store, IClock clock)
    {
        return new PendingVisitorAdjustmentWorkflow(
            store,
            PendingVisitorAdjustmentRules.DefaultPendingVisitorAdjustmentValidator(),
            clock,
            new List<string>(),
            PendingVisitorAdjustmentEngine.DefaultPendingVisitorAdjustmentEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending visitor adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingVisitorAdjustmentStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending visitor adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingVisitorAdjustmentStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending visitor adjustment {code} not found");
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
