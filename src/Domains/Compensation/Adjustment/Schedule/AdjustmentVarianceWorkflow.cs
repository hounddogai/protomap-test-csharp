namespace Acme.Workforce.Compensation.Adjustment.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentVarianceDesk(IAdjustmentVarianceStore Store, AdjustmentVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentVarianceStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentVarianceStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of adjustment variance workflow.</summary>
public static class AdjustmentVarianceWorkflow
{
    public static AdjustmentVarianceDesk NewAdjustmentVarianceDesk(IAdjustmentVarianceStore store, IClock clock)
    {
        return new AdjustmentVarianceDesk(
            store,
            AdjustmentVarianceRules.DefaultAdjustmentVarianceValidator(),
            clock,
            new List<string>());
    }
}
