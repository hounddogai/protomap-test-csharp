namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves equity grant adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record EquityGrantAdjustmentService(IEquityGrantAdjustmentStore Store, EquityGrantAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(EquityGrantAdjustmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"equity grant adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(EquityGrantAdjustmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of equity grant adjustment workflow.</summary>
public static class EquityGrantAdjustmentWorkflow
{
    public static EquityGrantAdjustmentService NewEquityGrantAdjustmentService(IEquityGrantAdjustmentStore store, IClock clock)
    {
        return new EquityGrantAdjustmentService(
            store,
            EquityGrantAdjustmentRules.DefaultEquityGrantAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
