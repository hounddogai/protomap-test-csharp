namespace Acme.Workforce.Compensation.Adjustment.AdjustmentAgreement;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves adjustment estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdjustmentEstimateDesk(IAdjustmentEstimateStore Store, AdjustmentEstimateValidator Validator, IClock Clock, List<string> Journal, AdjustmentEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdjustmentEstimateStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdjustmentEstimateStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adjustment estimate {code} not found");
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

/// <summary>Functions of adjustment estimate workflow.</summary>
public static class AdjustmentEstimateWorkflow
{
    public static AdjustmentEstimateDesk NewAdjustmentEstimateDesk(IAdjustmentEstimateStore store, IClock clock)
    {
        return new AdjustmentEstimateDesk(
            store,
            AdjustmentEstimateRules.DefaultAdjustmentEstimateValidator(),
            clock,
            new List<string>(),
            AdjustmentEstimateEngine.DefaultAdjustmentEstimateEngine());
    }
}
